using System;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO.Packaging;
using System.Xml.Linq;

namespace System_ATP_creator
{
    public class ATPGenerator
    {
        private readonly string referenceFolder = @"C:\Users\danielk\DanielSW\Projects\System ATP creator\ATP references";
        private readonly string masterATPPath;

        public ATPGenerator()
        {
            // Use the correct template: METS GENERIC ATP_LD notes.doc (preferred) / .docx / .xml
            string appBase = AppDomain.CurrentDomain.BaseDirectory;
            string docPath = Path.Combine(appBase, "METS GENERIC ATP_LD notes.doc");
            string docxPath = Path.Combine(appBase, "METS GENERIC ATP_LD notes.docx");
            string xmlPath = Path.Combine(appBase, "METS GENERIC ATP_LD notes.xml");

            if (File.Exists(docPath))
            {
                masterATPPath = docPath;
            }
            else if (File.Exists(docxPath))
            {
                masterATPPath = docxPath;
            }
            else if (File.Exists(xmlPath))
            {
                masterATPPath = xmlPath;
            }
            else
            {
                // Fallback to project folder
                string projectFolder = @"C:\Users\danielk\DanielSW\Projects\System ATP creator\System ATP creator";
                string projectDocPath = Path.Combine(projectFolder, "METS GENERIC ATP_LD notes.doc");
                string projectDocxPath = Path.Combine(projectFolder, "METS GENERIC ATP_LD notes.docx");
                string projectXmlPath = Path.Combine(projectFolder, "METS GENERIC ATP_LD notes.xml");

                if (File.Exists(projectDocPath))
                    masterATPPath = projectDocPath;
                else if (File.Exists(projectDocxPath))
                    masterATPPath = projectDocxPath;
                else if (File.Exists(projectXmlPath))
                    masterATPPath = projectXmlPath;
                else
                    masterATPPath = "";
            }
        }

        public string GenerateATP(ATPConfiguration config)
        {
            // Create output filename: ATP_METS_S16_A456121000010.docx
            string fileName = $"ATP_{config.SystemType}";

            // For ILET, don't include variant in filename
            if (!config.SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(config.METSVariant))
                fileName += $"_{config.METSVariant}";

            // Add FOV/Aperture (remove quotes and "FOV" prefix)
            string aperture = config.FOV.Replace("\"", "");
            fileName += $"_{aperture}";

            // Add Order Number with "A" prefix and "1000010" suffix
            if (!string.IsNullOrEmpty(config.OrderNumber))
            {
                fileName += $"_A{config.OrderNumber}1000010";
            }
            else
            {
                // If no order number, use default
                fileName += "_A1000010";
            }

            // Output path - use custom save path if provided, otherwise use Documents folder
            string outputFolder = !string.IsNullOrEmpty(config.SavePath) 
                ? config.SavePath 
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Generated ATPs");
            
            // Ensure the output folder exists and is accessible
            try
            {
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot create or access the output folder '{outputFolder}'.\n\nError: {ex.Message}\n\nPlease choose a different save location.", ex);
            }

            // Always output docx
            string outputPath = Path.Combine(outputFolder, fileName + ".docx");

            // Check if master ATP exists
            if (!string.IsNullOrEmpty(masterATPPath) && File.Exists(masterATPPath))
            {
                string templateDocxPath = GetTemplateDocxPath();
                if (!string.IsNullOrEmpty(templateDocxPath) && File.Exists(templateDocxPath))
                {
                    File.Copy(templateDocxPath, outputPath, true);
                    try
                    {
                        ModifyATPDocument(outputPath, config);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not modify template: {ex.Message}");
                    }
                }
                else
                {
                    CreateNewATPDocument(outputPath, config);
                }
            }
            else
            {
                // Create a new ATP document from scratch if template doesn't exist
                CreateNewATPDocument(outputPath, config);
            }

            return outputPath;
        }

        private string GetTemplateDocxPath()
        {
            string extension = Path.GetExtension(masterATPPath).ToLowerInvariant();
            if (extension == ".docx")
                return masterATPPath;

            if (extension == ".doc")
            {
                string tempDocxPath = Path.Combine(Path.GetTempPath(), $"MasterATP_{Guid.NewGuid():N}.docx");
                if (DocConverter.ConvertDocToDocx(masterATPPath, tempDocxPath))
                {
                    return tempDocxPath;
                }
            }

            if (extension == ".xml")
            {
                string tempDocxPath = Path.Combine(Path.GetTempPath(), $"MasterATP_{Guid.NewGuid():N}.docx");
                ConvertFlatOpcToDocx(masterATPPath, tempDocxPath);
                return tempDocxPath;
            }

            return masterATPPath;
        }

        private void ConvertFlatOpcToDocx(string xmlPath, string docxPath)
        {
            XDocument flatOpc = XDocument.Load(xmlPath);
            XNamespace pkg = "http://schemas.microsoft.com/office/2006/xmlPackage";

            using (Package package = Package.Open(docxPath, FileMode.Create))
            {
                foreach (XElement part in flatOpc.Root?.Elements(pkg + "part") ?? Array.Empty<XElement>())
                {
                    string? partName = part.Attribute(pkg + "name")?.Value;
                    string? contentType = part.Attribute(pkg + "contentType")?.Value;
                    if (string.IsNullOrEmpty(partName) || string.IsNullOrEmpty(contentType))
                        continue;

                    Uri partUri = PackUriHelper.CreatePartUri(new Uri(partName, UriKind.Relative));
                    PackagePart packagePart = package.CreatePart(partUri, contentType, CompressionOption.Normal);

                    XElement? xmlData = part.Element(pkg + "xmlData");
                    if (xmlData != null)
                    {
                        XElement? rootElement = xmlData.Elements().FirstOrDefault();
                        if (rootElement != null)
                        {
                            using Stream partStream = packagePart.GetStream(FileMode.Create, FileAccess.Write);
                            rootElement.Save(partStream);
                        }
                        continue;
                    }

                    XElement? binaryData = part.Element(pkg + "binaryData");
                    if (binaryData != null)
                    {
                        byte[] bytes = Convert.FromBase64String(binaryData.Value);
                        using Stream partStream = packagePart.GetStream(FileMode.Create, FileAccess.Write);
                        partStream.Write(bytes, 0, bytes.Length);
                    }
                }
            }

        }

        private void RemoveSectionByTitle(Body body, string titleStartsWith)
        {
            Paragraph? startPara = null;
            foreach (var para in body.Descendants<Paragraph>())
            {
                if (para.InnerText.Trim().StartsWith(titleStartsWith, StringComparison.OrdinalIgnoreCase))
                {
                    startPara = para;
                    break;
                }
            }

            if (startPara == null) return;

            var toRemove = new List<OpenXmlElement>();
            OpenXmlElement? current = startPara;
            while (current != null)
            {
                if (current is Paragraph p && p != startPara)
                {
                    string text = p.InnerText.Trim();
                    if (text.StartsWith("3.", StringComparison.OrdinalIgnoreCase) && !text.StartsWith(titleStartsWith, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                toRemove.Add(current);
                current = current.NextSibling();
            }

            foreach (var element in toRemove)
            {
                element.Remove();
            }
        }

        private void UpdateFieldsOnOpen(WordprocessingDocument wordDoc)
        {
            try
            {
                // Add or update document settings to update fields on open
                var settingsPart = wordDoc.MainDocumentPart?.DocumentSettingsPart;
                
                if (settingsPart == null)
                {
                    settingsPart = wordDoc.MainDocumentPart?.AddNewPart<DocumentSettingsPart>();
                    settingsPart.Settings = new DocumentFormat.OpenXml.Wordprocessing.Settings();
                }

                var settings = settingsPart.Settings;
                
                if (settings == null)
                {
                    settings = new DocumentFormat.OpenXml.Wordprocessing.Settings();
                    settingsPart.Settings = settings;
                }
                
                // Remove existing UpdateFieldsOnOpen setting if present
                var existingUpdateFields = settings.Elements<DocumentFormat.OpenXml.Wordprocessing.UpdateFieldsOnOpen>().FirstOrDefault();
                existingUpdateFields?.Remove();
                
                // Add UpdateFieldsOnOpen setting set to true with proper OnOffValue
                var updateFieldsOnOpen = new DocumentFormat.OpenXml.Wordprocessing.UpdateFieldsOnOpen();
                updateFieldsOnOpen.Val = new OnOffValue(true);
                settings.AppendChild(updateFieldsOnOpen);
                
                settings.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not set UpdateFieldsOnOpen: {ex.Message}");
            }
        }

        private void ModifyATPDocument(string filePath, ATPConfiguration config)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, true))
            {
                var body = wordDoc.MainDocumentPart?.Document.Body;
                if (body == null) return;

                System.Diagnostics.Debug.WriteLine("=== ModifyATPDocument START ===");
                System.Diagnostics.Debug.WriteLine($"HasBB: {config.HasBB}, BBSize: {config.BBSize}");
                System.Diagnostics.Debug.WriteLine($"HasIS: {config.HasIS}");
                System.Diagnostics.Debug.WriteLine($"HasLOSLaser: {config.HasLOSLaser}");

                // 1. Replace red text placeholders with user input values and change to black
                ReplaceRedTextPlaceholders(body, config, wordDoc);

                // 2. Add component bullets to Major Components section (page 4)
                AddMajorComponentsBullets(body, config);

                // 3. Add component test sections and update tables
                System.Diagnostics.Debug.WriteLine("=== Calling AddComponentTestSections ===");
                AddComponentTestSections(body, config);
                System.Diagnostics.Debug.WriteLine("=== AddComponentTestSections COMPLETE ===");
                System.Diagnostics.Debug.WriteLine($"_componentSections count: {_componentSections?.Count ?? 0}");
                
                // 4. Update the ATR (Results) table on page 9 - MUST be called AFTER AddComponentTestSections
                System.Diagnostics.Debug.WriteLine("=== Calling UpdateATRTable ===");
                UpdateATRTable(body, config);
                System.Diagnostics.Debug.WriteLine("=== UpdateATRTable COMPLETE ===");

                // 5. Fix alignment: Left-align everything EXCEPT page 1 and page 8
                FixDocumentAlignment(body);

                // 6. Populate Table 3: System Targets with CSV data
                if (!string.IsNullOrEmpty(config.TargetWheelPDFPath))
                {
                    PopulateSystemTargetsTable(body, config.TargetWheelPDFPath);
                }
                else
                {
                    // No CSV file uploaded - clean placeholder text from System Targets table
                    CleanSystemTargetsTable(body);
                }

                // 7. Update all fields (including Table of Contents) when document is opened
                UpdateFieldsOnOpen(wordDoc);

                // 8. Organize page breaks to prevent sections from splitting across pages
                OrganizePageBreaks(body);

                wordDoc.MainDocumentPart.Document.Save();
                System.Diagnostics.Debug.WriteLine("=== ModifyATPDocument END ===");
            }
        }

        private void AddMajorComponentsBullets(Body body, ATPConfiguration config)
        {
            // Find the marker text "Add New bullets from here" in the Major Components section
            Paragraph? markerPara = null;
            Paragraph? templateBulletPara = null;

            foreach (var para in body.Descendants<Paragraph>())
            {
                string paraText = para.InnerText.Trim();

                // Find an existing bullet to copy formatting from (like "METS Collimator Assembly" or "ILET Collimator Assembly")
                if (templateBulletPara == null && paraText.Contains("Collimator Assembly"))
                {
                    templateBulletPara = para;
                }

                if (paraText.Contains("Add New bullets from here"))
                {
                    markerPara = para;
                    break;
                }
            }

            if (markerPara == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Could not find 'Add New bullets from here' marker in Major Components section.");
                return;
            }

            // Insert component bullets after the marker
            OpenXmlElement? insertAfter = markerPara;

            // B.B (Blackbody)
            if (config.HasBB && !string.IsNullOrEmpty(config.BBSize) && !string.IsNullOrEmpty(config.BBType))
            {
                string bulletText;
                // Special formatting for SR200N-33 type
                if (config.BBType.Equals("SR200N-33", StringComparison.OrdinalIgnoreCase))
                {
                    bulletText = "IR Source: SR200N-33 Blackbody + Controller.";
                }
                else
                {
                    // For RR or STD types
                    bulletText = $"IR Source: SR800N-{config.BBSize}-{config.BBType} Blackbody + Controller.";
                }
                
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);
                
                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // I.S (Integrating Sphere)
            if (config.HasIS && !string.IsNullOrEmpty(config.ISExitAperture))
            {
                string aperture = config.ISExitAperture.Replace("\"", "");
                string bulletText = $"VIS Source: SR300N-{aperture}\" integrating sphere + Controller.";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);
                
                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Source Stage
            if (config.HasSourceStage)
            {
                string bulletText = "Motorized Source Stage.";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);
                
                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Focus Stage
            if (config.HasFocusStage)
            {
                string bulletText = "Motorized Focus stage.";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);
                
                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // XY Stage
            if (config.HasXYStage)
            {
                string bulletText = "XY Stage";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);

                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // NewPort Stage
            if (config.HasNewPortStage)
            {
                string bulletText = "NewPort Stage";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);

                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Backlight
            if (config.HasBacklight && !string.IsNullOrEmpty(config.BacklightType))
            {
                string bulletText = config.BacklightType.Equals("LED", StringComparison.OrdinalIgnoreCase) 
                    ? "Backlight LED." 
                    : "Fiber Optic Back Light.";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);

                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // QTH Lamp
            if (config.HasQTHLamp)
            {
                string bulletText = "QTH Lamp";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);

                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // LOS Alignment Target
            if (config.HasLOSAlignmentTarget)
            {
                string bulletText = "Target LOS with CCD&LED.";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);
                
                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // LOS Laser
            if (config.HasLOSLaser)
            {
                string bulletText = "LOS laser diode.";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);
                
                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Power Meter
            if (config.HasPowerMeter)
            {
                string bulletText = "Power Meter.";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);

                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Energy Meter
            if (config.HasEnergyMeter)
            {
                string bulletText = "Energy Meter.";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);

                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // CTE
            if (config.HasCTE)
            {
                string bulletText = "PC + CTE";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);
                
                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Gimbal
            if (config.HasGimbal)
            {
                string bulletText = "Gimbal.";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);
                
                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // VRS
            if (config.HasVRS)
            {
                string bulletText = "VRS";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);
                
                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Device Center
            if (config.HasDeviceCenter)
            {
                string bulletText = "Device Center";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);

                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Manual Choke
            if (config.HasManualChoke)
            {
                string bulletText = "Manual Choke";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);

                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Rackmount
            if (config.HasRackmount)
            {
                string bulletText = "Rackmount";
                Paragraph bulletPara = CreateBulletParagraph(bulletText, templateBulletPara);

                if (insertAfter?.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(bulletPara, insertAfter);
                    insertAfter = bulletPara;
                }
            }

            // Remove the marker paragraph after inserting all bullets
            markerPara.Remove();
        }

        private Paragraph CreateBulletParagraph(string text, Paragraph? templateBullet)
        {
            Paragraph para = new Paragraph();

            // Copy paragraph properties from template bullet if available
            if (templateBullet?.ParagraphProperties != null)
            {
                ParagraphProperties templateProps = templateBullet.ParagraphProperties;
                ParagraphProperties paraProps = (ParagraphProperties)templateProps.CloneNode(true);

                // Ensure left alignment
                var justification = paraProps.Elements<Justification>().FirstOrDefault();
                if (justification != null)
                {
                    justification.Val = JustificationValues.Left;
                }
                else
                {
                    paraProps.Append(new Justification() { Val = JustificationValues.Left });
                }

                // Ensure LTR direction
                var bidi = paraProps.Elements<BiDi>().FirstOrDefault();
                if (bidi != null)
                {
                    bidi.Val = new OnOffValue(false);
                }
                else
                {
                    paraProps.Append(new BiDi() { Val = new OnOffValue(false) });
                }

                para.Append(paraProps);
            }
            else
            {
                // Fallback: create bullet formatting manually
                ParagraphProperties paraProps = new ParagraphProperties();

                // Add bullet/numbering properties
                NumberingProperties numProps = new NumberingProperties();
                numProps.Append(new NumberingLevelReference() { Val = 0 });
                numProps.Append(new NumberingId() { Val = 1 }); // Use numbering definition 1 (typically bullets)
                paraProps.Append(numProps);

                paraProps.Append(new ParagraphStyleId() { Val = "Normal" });
                paraProps.Append(new Justification() { Val = JustificationValues.Left });
                paraProps.Append(new BiDi() { Val = new OnOffValue(false) });
                para.Append(paraProps);
            }

            Run run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);

            return para;
        }

        private void FixDocumentAlignment(Body body)
        {
            // STEP 1: LEFT-ALIGN EVERYTHING (including tables)
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                var paraProps = paragraph.ParagraphProperties;
                if (paraProps == null)
                {
                    paraProps = new ParagraphProperties();
                    paragraph.PrependChild(paraProps);
                }

                var existingJustification = paraProps.Elements<Justification>().FirstOrDefault();
                existingJustification?.Remove();
                paraProps.Append(new Justification() { Val = JustificationValues.Left });
            }
            
            // STEP 2: Now center ONLY page 1 and page 8
            bool onTitlePage = true;
            bool foundTableOfContent = false;
            bool inATR = false;
            int paragraphsInATR = 0;
            
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                string paraText = paragraph.InnerText.Trim();
                
                // Mark when we reach Table Of Content (end of title page)
                if (paraText.Contains("Table Of Content") || paraText.Contains("LIST OF TESTS"))
                {
                    onTitlePage = false;
                    foundTableOfContent = true;
                }
                
                // Check if we're entering the ATR section (case-insensitive)
                if (paraText.IndexOf("ACCEPTANCE TEST RESULTS", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    paraText.IndexOf("Acceptance Test Results", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    inATR = true;
                    paragraphsInATR = 0;
                }
                
                // Count paragraphs while in ATR
                if (inATR)
                {
                    paragraphsInATR++;
                    
                    // Exit ATR after 30 paragraphs (should be enough for the entire ATR page)
                    if (paragraphsInATR > 30)
                    {
                        inATR = false;
                    }
                }
                
                // Get or create paragraph properties
                var paraProps = paragraph.ParagraphProperties;
                if (paraProps == null)
                {
                    paraProps = new ParagraphProperties();
                    paragraph.PrependChild(paraProps);
                }
                
                var justification = paraProps.Elements<Justification>().FirstOrDefault();
                
                // Center if: we're on title page (before Table Of Content) OR we're in ATR section
                if (onTitlePage || inATR)
                {
                    if (justification != null)
                    {
                        justification.Val = JustificationValues.Center;
                    }
                    else
                    {
                        paraProps.Append(new Justification() { Val = JustificationValues.Center });
                    }
                }
            }
            
            // STEP 3: Also center tables on page 8 (ATR section)
            inATR = false;
            foreach (var table in body.Descendants<Table>())
            {
                // Check if this table is in ATR section by looking at surrounding paragraphs
                var prevPara = table.PreviousSibling<Paragraph>();
                var nextPara = table.NextSibling<Paragraph>();
                
                string surroundingText = "";
                if (prevPara != null) surroundingText += prevPara.InnerText;
                if (nextPara != null) surroundingText += nextPara.InnerText;
                
                // If we find ACCEPTANCE TEST RESULTS near this table, center all its content
                if (surroundingText.Contains("ACCEPTANCE TEST RESULTS") || surroundingText.Contains("ATR"))
                {
                    foreach (var cell in table.Descendants<TableCell>())
                    {
                        foreach (var para in cell.Elements<Paragraph>())
                        {
                            var paraProps = para.ParagraphProperties;
                            if (paraProps == null)
                            {
                                paraProps = new ParagraphProperties();
                                para.PrependChild(paraProps);
                            }

                            var existingJustification = paraProps.Elements<Justification>().FirstOrDefault();
                            existingJustification?.Remove();
                            paraProps.Append(new Justification() { Val = JustificationValues.Center });
                        }
                    }
                }
            }
        }

        private void SetLeftAlignment(Body body)
        {
            bool passedFirstPage = false;
            
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                string paraText = paragraph.InnerText.Trim();
                
                // Skip first page (title page) - keep centered
                if (!passedFirstPage)
                {
                    if (paraText.Contains("LIST OF TESTS") || paraText.Contains("Table Of Content"))
                    {
                        passedFirstPage = true;
                    }
                    continue; // Keep first page centered
                }
                
                // Skip ONLY "Table Of Content" heading - keep centered
                if (paraText.Equals("Table Of Content", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Keep this heading centered
                }
                
                // Left-align "LIST OF TESTS" section
                if (paraText.Equals("LIST OF TESTS", StringComparison.OrdinalIgnoreCase) ||
                    paraText.Contains("The ATP refers to the following tests") ||
                    paraText.Contains("Table 1: List of Tests"))
                {
                    // Apply left alignment to these
                }
                
                // Skip ATR page - will be centered separately
                if (paraText.Contains("ACCEPTANCE TEST RESULTS") || paraText.Contains("ATR"))
                {
                    continue; // Will be centered by CenterAlignATRPage
                }
                
                // Skip tables - handle them separately
                if (paragraph.Ancestors<Table>().Any())
                {
                    continue;
                }

                var paraProps = paragraph.ParagraphProperties;
                if (paraProps == null)
                {
                    paraProps = new ParagraphProperties();
                    paragraph.PrependChild(paraProps);
                }

                // Remove any existing justification
                var existingJustification = paraProps.Elements<Justification>().FirstOrDefault();
                existingJustification?.Remove();

                // Set to left alignment for content paragraphs
                paraProps.Append(new Justification() { Val = JustificationValues.Left });
            }
            
            // Also handle table cells - align them to left
            foreach (var table in body.Descendants<Table>())
            {
                foreach (var cell in table.Descendants<TableCell>())
                {
                    foreach (var para in cell.Elements<Paragraph>())
                    {
                        // Skip if this is in the ATR section
                        string cellText = cell.InnerText.Trim();
                        if (cellText.Contains("ACCEPTANCE TEST RESULTS") || cellText.Contains("ATR"))
                            continue;
                        
                        var paraProps = para.ParagraphProperties;
                        if (paraProps == null)
                        {
                            paraProps = new ParagraphProperties();
                            para.PrependChild(paraProps);
                        }

                        var existingJustification = paraProps.Elements<Justification>().FirstOrDefault();
                        existingJustification?.Remove();
                        paraProps.Append(new Justification() { Val = JustificationValues.Left });
                    }
                }
            }
        }

        private void CenterAlignATRPage(Body body)
        {
            bool inATRSection = false;

            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                string paraText = paragraph.InnerText.Trim();

                // Find the ATR section (page 9)
                if (paraText.Contains("ACCEPTANCE TEST RESULTS") || paraText.Contains("ATR"))
                {
                    inATRSection = true;
                }

                // Stop at the end of the document or next major section
                if (inATRSection && (paraText.StartsWith("APPENDIX") || paraText.Contains("End of Document")))
                {
                    inATRSection = false;
                }

                // Center align all paragraphs in the ATR section
                if (inATRSection)
                {
                    // Skip tables
                    if (paragraph.Ancestors<Table>().Any())
                    {
                        continue;
                    }

                    var paraProps = paragraph.ParagraphProperties;
                    if (paraProps == null)
                    {
                        paraProps = new ParagraphProperties();
                        paragraph.PrependChild(paraProps);
                    }

                    // Remove existing justification
                    var existingJustification = paraProps.Elements<Justification>().FirstOrDefault();
                    existingJustification?.Remove();

                    // Set to center alignment
                    paraProps.Append(new Justification() { Val = JustificationValues.Center });
                }
            }
        }

        private void OrganizePageBreaks(Body body)
        {
            System.Diagnostics.Debug.WriteLine("=== OrganizePageBreaks START ===");

            // Find all paragraphs containing "Below here in new page!" marker text
            // Add page break to the next paragraph and delete the marker

            var markersToRemove = new List<Paragraph>();

            foreach (var paragraph in body.Descendants<Paragraph>().ToList())
            {
                string paraText = paragraph.InnerText.Trim();

                // Check if this paragraph contains the page break marker
                if (paraText.Contains("Below here in new page!"))
                {
                    System.Diagnostics.Debug.WriteLine($"Found page break marker: '{paraText}'");

                    // Find the next paragraph
                    var nextElement = paragraph.NextSibling();

                    while (nextElement != null && !(nextElement is Paragraph))
                    {
                        nextElement = nextElement.NextSibling();
                    }

                    // Add page break to the next paragraph
                    if (nextElement is Paragraph nextPara)
                    {
                        System.Diagnostics.Debug.WriteLine("Adding page break to next paragraph");

                        var paraProps = nextPara.ParagraphProperties;
                        if (paraProps == null)
                        {
                            paraProps = new ParagraphProperties();
                            nextPara.PrependChild(paraProps);
                        }

                        // Add PageBreakBefore to start this paragraph on a new page
                        var pageBreakBefore = paraProps.Elements<PageBreakBefore>().FirstOrDefault();
                        if (pageBreakBefore == null)
                        {
                            paraProps.Append(new PageBreakBefore());
                        }
                    }

                    // Mark this marker paragraph for removal
                    markersToRemove.Add(paragraph);
                }
            }

            // Remove all marker paragraphs
            foreach (var marker in markersToRemove)
            {
                System.Diagnostics.Debug.WriteLine("Removing page break marker paragraph");
                marker.Remove();
            }

            System.Diagnostics.Debug.WriteLine($"=== OrganizePageBreaks END (processed {markersToRemove.Count} page break markers) ===");
        }

        private void AddPageBreakBefore(Paragraph paragraph)
        {
            var paraProps = paragraph.ParagraphProperties;
            if (paraProps == null)
            {
                paraProps = new ParagraphProperties();
                paragraph.PrependChild(paraProps);
            }

            // Add page break before this paragraph
            var pageBreakBefore = paraProps.Elements<PageBreakBefore>().FirstOrDefault();
            if (pageBreakBefore == null)
            {
                paraProps.Append(new PageBreakBefore());
            }
        }

        private void AddPageBreakAfter(Paragraph paragraph)
        {
            // Instead of inserting a new paragraph, add a page break to the next sibling if it exists
            var nextElement = paragraph.NextSibling();

            // Find the next paragraph
            while (nextElement != null && !(nextElement is Paragraph))
            {
                nextElement = nextElement.NextSibling();
            }

            // If found, add page break to it
            if (nextElement is Paragraph nextPara)
            {
                var paraProps = nextPara.ParagraphProperties;
                if (paraProps == null)
                {
                    paraProps = new ParagraphProperties();
                    nextPara.PrependChild(paraProps);
                }

                // Only add page break if not already present
                var pageBreakBefore = paraProps.Elements<PageBreakBefore>().FirstOrDefault();
                if (pageBreakBefore == null)
                {
                    paraProps.Append(new PageBreakBefore());
                }
            }
        }

        private void EnsureSectionNotSplit(Paragraph sectionHeading)
        {
            // Simplified - no automatic section protection
            // User will manage page breaks manually
        }

        private void KeepTableTogether(Table table)
        {
            // Simplified - no automatic table protection
            // User will manage page breaks manually
        }

        private void AddCablesContent(Body body)
        {
            // Find the Cables section (3.9)
            Paragraph? cablesPara = null;
            foreach (var para in body.Descendants<Paragraph>())
            {
                string paraText = para.InnerText.Trim();
                if (paraText.Contains("3.9") && paraText.Contains("Cables"))
                {
                    cablesPara = para;
                    break;
                }
            }

            if (cablesPara == null) return;

            // Add Cables content after the heading
            string[] cablesContent = new string[]
            {
                "Inspect the following and verify its condition:",
                " External casing.",
                " Overall cleanness.",
                " Marking & Identification labels on cable.",
                "Write Pass/Fail in the ATR document."
            };

            OpenXmlElement insertAfter = cablesPara;
            foreach (string line in cablesContent)
            {
                Paragraph newPara = new Paragraph();
                ParagraphProperties paraProps = new ParagraphProperties();
                paraProps.Append(new ParagraphStyleId() { Val = "Normal" });
                paraProps.Append(new Justification() { Val = JustificationValues.Left });
                
                newPara.Append(paraProps);
                Run run = new Run(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
                newPara.Append(run);

                if (insertAfter.Parent != null)
                {
                    insertAfter.Parent.InsertAfter(newPara, insertAfter);
                    insertAfter = newPara;
                }
            }
        }

        private void AddComponentTestSections(Body body, ATPConfiguration config)
        {
            System.Diagnostics.Debug.WriteLine("=== AddComponentTestSections START ===");
            
            // Collect all component sections based on configuration
            var sections = new List<ComponentSection>();

            // Target Wheel is always included
            sections.Add(ComponentSections.TargetWheel());

            if (config.HasSourceStage)
                sections.Add(ComponentSections.SourceStage());

            if (config.HasFocusStage && 
                (!string.IsNullOrEmpty(config.FocusStageFiniteDistance) || 
                 !string.IsNullOrEmpty(config.FocusStageFiniteDistance2) || 
                 !string.IsNullOrEmpty(config.FocusStageFiniteDistance3)))
                sections.Add(ComponentSections.FocusStage(
                    config.FocusStageFiniteDistance, 
                    config.FocusStageFiniteDistance2, 
                    config.FocusStageFiniteDistance3, 
                    config.SystemType, 
                    config.METSVariant));

            if (config.HasBB && !string.IsNullOrEmpty(config.BBType) && !string.IsNullOrEmpty(config.BBSize))
            {
                System.Diagnostics.Debug.WriteLine($"Adding BB section: Type={config.BBType}, Size={config.BBSize}");
                sections.Add(ComponentSections.Blackbody(config.BBType, config.BBSize));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"NOT adding BB: HasBB={config.HasBB}, BBType={config.BBType}, BBSize={config.BBSize}");
            }

            if (config.HasIS && !string.IsNullOrEmpty(config.ISExitAperture))
            {
                string aperture = config.ISExitAperture.Replace("\"", "");
                sections.Add(ComponentSections.IntegratingSphere(aperture));
            }

            if (config.HasLOSLaser)
                sections.Add(ComponentSections.LOSLaser());

            if (config.HasBacklight && !string.IsNullOrEmpty(config.BacklightType))
                sections.Add(ComponentSections.Backlight(config.BacklightType));

            if (config.HasQTHLamp)
                sections.Add(ComponentSections.QTHLamp());

            if (config.HasXYStage)
                sections.Add(ComponentSections.XYStage());

            if (config.HasPowerMeter)
                sections.Add(ComponentSections.PowerMeter());

            if (config.HasEnergyMeter)
                sections.Add(ComponentSections.EnergyMeter());

            if (config.HasManualChoke)
                sections.Add(ComponentSections.ManualChoke());

            if (config.HasCTE)
            {
                sections.Add(ComponentSections.CTE());
                sections.Add(ComponentSections.CTEInitiatedSelfTest()); // Add second section for CTE
            }

            if (config.HasRackmount)
                sections.Add(ComponentSections.Rackmount());

            if (config.HasGimbal && !string.IsNullOrEmpty(config.GimbalSize))
                sections.Add(ComponentSections.Gimbal(config.GimbalSize));

            if (config.HasLOSAlignmentTarget)
                sections.Add(ComponentSections.LOSAlignmentTarget());

            if (config.HasDeviceCenter)
                sections.Add(ComponentSections.DeviceCenter());

            if (config.HasVRS)
            {
                string vrs1 = string.IsNullOrEmpty(config.VRS1) ? "NA" : config.VRS1;
                string vrs2 = string.IsNullOrEmpty(config.VRS2) ? "NA" : config.VRS2;
                string vrs3 = string.IsNullOrEmpty(config.VRS3) ? "NA" : config.VRS3;
                sections.Add(ComponentSections.VRS(vrs1, vrs2, vrs3));
            }

            if (config.HasNewPortStage && !string.IsNullOrEmpty(config.NewPortStageMaxWeight))
                sections.Add(ComponentSections.NewPortStage(config.NewPortStageMaxWeight));

            System.Diagnostics.Debug.WriteLine($"Total sections collected: {sections.Count}");
            foreach (var section in sections)
            {
                System.Diagnostics.Debug.WriteLine($"  - {section.ListOfTestsEntry}");
            }

            // Remove Blackbody section when not selected, and insert before Cables when selected
            if (!config.HasBB)
                RemoveSectionByTitle(body, "SR800N-4D Blackbody");

            // Update List of Tests table with component entries
            UpdateListOfTestsTable(body, sections, config);

            // Insert full test procedure sections
            System.Diagnostics.Debug.WriteLine("=== Calling InsertComponentSectionsIntoDocument ===");
            InsertComponentSectionsIntoDocument(body, sections);
            
            // Store sections for ATR table update
            _componentSections = sections;
        }
        
        private List<ComponentSection>? _componentSections;

        private void UpdateTableOfContents(Body body, List<ComponentSection> sections)
        {
            // Intentionally left empty. The Word TOC will reflect only paragraphs
            // styled as headings. Component sections apply Heading2 for the title
            // and Normal/outline level 9 for all sub-items to keep them out of the TOC.
        }

        private void UpdateListOfTestsTable(Body body, List<ComponentSection> sections, ATPConfiguration config)
        {
            // Find the "List of Tests" table
            Table? listOfTestsTable = null;
            foreach (var table in body.Descendants<Table>())
            {
                string tableText = table.InnerText;
                if (tableText.Contains("Test No.") && tableText.Contains("Test Name"))
                {
                    listOfTestsTable = table;
                    break;
                }
            }

            if (listOfTestsTable == null) return;

            // Find the row with "Cables" - we'll insert new rows AFTER it
            TableRow? cablesRow = null;

            foreach (var row in listOfTestsTable.Elements<TableRow>())
            {
                string rowText = row.InnerText;
                
                if (rowText.Contains("Cables"))
                {
                    cablesRow = row;
                    break;
                }
            }

            if (cablesRow == null) return;

            // Insert component test rows starting from test 10 (after Cables which is test 9)
            int testNum = 10; // Start from 10
            TableRow? currentInsertAfter = cablesRow;

            foreach (var section in sections)
            {
                // Skip Target Wheel as it's already in the template
                if (section.TableOfContentsEntry.Contains("Target Wheel")) continue;

                TableRow newRow = CreateTestTableRow(testNum.ToString(), section.ListOfTestsEntry, "Test", GetTestNotes(section, config));
                
                if (currentInsertAfter != null && currentInsertAfter.Parent != null)
                {
                    currentInsertAfter.Parent.InsertAfter(newRow, currentInsertAfter);
                    currentInsertAfter = newRow; // Update to insert after this new row
                }

                testNum++;
            }
            
            // Cables row stays as test #9, no need to renumber
            // Just ensure left alignment for all cells in the Cables row
            if (cablesRow != null && cablesRow.Elements<TableCell>().Any())
            {
                foreach (var cell in cablesRow.Elements<TableCell>())
                {
                    foreach (var para in cell.Elements<Paragraph>())
                    {
                        var paraProps = para.ParagraphProperties;
                        if (paraProps == null)
                        {
                            paraProps = new ParagraphProperties();
                            para.PrependChild(paraProps);
                        }
                        var existingJust = paraProps.Elements<Justification>().FirstOrDefault();
                        existingJust?.Remove();
                        paraProps.Append(new Justification() { Val = JustificationValues.Left });
                    }
                }
            }
        }

        private string GetTestNotes(ComponentSection section, ATPConfiguration config)
        {
            // Return specific notes for certain components
            // Note: Per requirements, BB should have empty notes column
            // if (section.ListOfTestsEntry.Contains("SR-800N"))
            //     return "TA-704-100-0010";
            
            return "";
        }

        private TableRow CreateTestTableRow(string testNo, string testName, string testMethod, string notes)
        {
            TableRow row = new TableRow();

            // Test No. cell
            TableCell cell1 = new TableCell();
            Paragraph para1 = new Paragraph();
            ParagraphProperties paraProps1 = new ParagraphProperties();
            paraProps1.Append(new Justification() { Val = JustificationValues.Left });
            paraProps1.Append(new BiDi() { Val = new OnOffValue(false) }); // Force LTR direction
            para1.Append(paraProps1);
            para1.Append(new Run(new Text(testNo + ".") { Space = SpaceProcessingModeValues.Preserve }));
            cell1.Append(para1);
            row.Append(cell1);

            // Test Name cell
            TableCell cell2 = new TableCell();
            Paragraph para2 = new Paragraph();
            ParagraphProperties paraProps2 = new ParagraphProperties();
            paraProps2.Append(new Justification() { Val = JustificationValues.Left });
            paraProps2.Append(new BiDi() { Val = new OnOffValue(false) }); // Force LTR direction
            para2.Append(paraProps2);
            para2.Append(new Run(new Text(testName) { Space = SpaceProcessingModeValues.Preserve }));
            cell2.Append(para2);
            row.Append(cell2);

            // Test Method cell
            TableCell cell3 = new TableCell();
            Paragraph para3 = new Paragraph();
            ParagraphProperties paraProps3 = new ParagraphProperties();
            paraProps3.Append(new Justification() { Val = JustificationValues.Left });
            paraProps3.Append(new BiDi() { Val = new OnOffValue(false) }); // Force LTR direction
            para3.Append(paraProps3);
            para3.Append(new Run(new Text(testMethod) { Space = SpaceProcessingModeValues.Preserve }));
            cell3.Append(para3);
            row.Append(cell3);

            // Notes cell
            TableCell cell4 = new TableCell();
            Paragraph para4 = new Paragraph();
            ParagraphProperties paraProps4 = new ParagraphProperties();
            paraProps4.Append(new Justification() { Val = JustificationValues.Left });
            paraProps4.Append(new BiDi() { Val = new OnOffValue(false) }); // Force LTR direction
            para4.Append(paraProps4);
            para4.Append(new Run(new Text(notes) { Space = SpaceProcessingModeValues.Preserve }));
            cell4.Append(para4);
            row.Append(cell4);

            return row;
        }

        private void InsertComponentSectionsIntoDocument(Body body, List<ComponentSection> sections)
        {
            // Insert sections at the marker text in the template.
            const string markerText = "Start planting new sections from here!";

            var markerPara = body.Descendants<Paragraph>()
                .FirstOrDefault(p => p.InnerText.Contains(markerText));

            if (markerPara == null)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Could not find marker text '{markerText}'.");
                return;
            }

            OpenXmlElement? currentInsertAfter = markerPara;

            foreach (var section in sections)
            {
                // Skip Target Wheel as it's already in the template
                if (section.TableOfContentsEntry.Contains("Target Wheel"))
                    continue;

                string sectionText = section.TestProcedureSection;
                string[] lines = sectionText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    Paragraph newPara = new Paragraph();
                    ParagraphProperties paraProps = new ParagraphProperties();

                    if (i == 0)
                    {
                        // First line is the main heading - BOLD with Heading2 style (for automatic numbering)
                        paraProps.Append(new ParagraphStyleId() { Val = "Heading2" });
                        paraProps.Append(new Justification() { Val = JustificationValues.Left });
                        paraProps.Append(new BiDi() { Val = new OnOffValue(false) }); // Ensure LTR direction
                        
                        // Add TAB at the start of text to create spacing after auto-number
                        Run newRun = new Run(new Text("\t" + line.Trim()) { Space = SpaceProcessingModeValues.Preserve });
                        newPara.Append(paraProps);
                        newPara.AppendChild(newRun);
                    }
                    else if (line.Trim().Equals("Test Procedure:", StringComparison.OrdinalIgnoreCase))
                    {
                        // "Test Procedure:" - NO style, plain paragraph, LEFT aligned
                        // Don't use any ParagraphStyleId to avoid inheriting unwanted formatting
                        paraProps.Append(new Justification() { Val = JustificationValues.Left });
                        paraProps.Append(new Indentation() { Left = "0", Right = "0" }); // No indentation
                        paraProps.Append(new BiDi() { Val = new OnOffValue(false) }); // Force LTR, not RTL
                        
                        // Create completely plain run with no formatting
                        Run newRun = new Run(new Text(line.Trim()) { Space = SpaceProcessingModeValues.Preserve });
                        
                        newPara.Append(paraProps);
                        newPara.AppendChild(newRun);
                    }
                    else
                    {
                        // Subsections - Normal style (NOT bold) with Heading3 for automatic numbering
                        paraProps.Append(new ParagraphStyleId() { Val = "Heading3" });
                        paraProps.Append(new Justification() { Val = JustificationValues.Left });
                        paraProps.Append(new Indentation() { Left = "720" }); // Indent by 1 tab (720 twips = 0.5 inch)
                        
                        // Create run with explicit non-bold formatting to override Heading3 style
                        Run newRun = new Run();
                        RunProperties runProps = new RunProperties();
                        runProps.Append(new Bold() { Val = new OnOffValue(false) }); // Explicitly set NOT bold
                        newRun.Append(runProps);
                        newRun.Append(new Text(line.Trim()) { Space = SpaceProcessingModeValues.Preserve });
                        
                        newPara.Append(paraProps);
                        newPara.AppendChild(newRun);
                    }

                    // Insert after current position
                    if (currentInsertAfter?.Parent != null)
                    {
                        currentInsertAfter.Parent.InsertAfter(newPara, currentInsertAfter);
                        currentInsertAfter = newPara;
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    Paragraph spacingPara = new Paragraph();
                    ParagraphProperties spacingProps = new ParagraphProperties();
                    spacingProps.Append(new Justification() { Val = JustificationValues.Left });
                    spacingPara.Append(spacingProps);

                    if (currentInsertAfter?.Parent != null)
                    {
                        currentInsertAfter.Parent.InsertAfter(spacingPara, currentInsertAfter);
                        currentInsertAfter = spacingPara;
                    }
                }
            }

            // Remove the marker paragraph after inserting sections.
            markerPara.Remove();
        }

        private void ReplaceYellowHighlightedText(Body body, ATPConfiguration config)
        {
            var paragraphsProcessed = new HashSet<Paragraph>();

            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                string paraText = paragraph.InnerText;

                // Handle all S/N variations - check for any mention of S/N or Serial Number
                if ((paraText.Contains("S/N") || paraText.Contains("Serial Number") || paraText.Contains("System S/N")) 
                    && !paragraphsProcessed.Contains(paragraph))
                {
                    var yellowRuns = paragraph.Descendants<Run>()
                        .Where(r => r.RunProperties?.Highlight?.Val?.HasValue == true && 
                                   r.RunProperties.Highlight.Val == HighlightColorValues.Yellow)
                        .ToList();

                    if (yellowRuns.Any())
                    {
                        // Replace first yellow run with user S/N, remove the rest
                        bool first = true;
                        foreach (var run in yellowRuns)
                        {
                            if (first && !string.IsNullOrEmpty(config.SystemSN))
                            {
                                foreach (var text in run.Elements<Text>())
                                {
                                    text.Text = config.SystemSN;
                                }
                                run.RunProperties.Highlight = null;
                                first = false;
                            }
                            else
                            {
                                run.Remove();
                            }
                        }
                        paragraphsProcessed.Add(paragraph);
                    }
                }
            }

            // Also check in table cells for S/N (page 9 might be in a table)
            foreach (var tableCell in body.Descendants<TableCell>())
            {
                string cellText = tableCell.InnerText;
                
                if (cellText.Contains("S/N") || cellText.Contains("Serial Number") || cellText.Contains("System S/N"))
                {
                    foreach (var paragraph in tableCell.Descendants<Paragraph>())
                    {
                        if (paragraphsProcessed.Contains(paragraph)) continue;

                        var yellowRuns = paragraph.Descendants<Run>()
                            .Where(r => r.RunProperties?.Highlight?.Val?.HasValue == true && 
                                       r.RunProperties.Highlight.Val == HighlightColorValues.Yellow)
                            .ToList();

                        if (yellowRuns.Any())
                        {
                            bool first = true;
                            foreach (var run in yellowRuns)
                            {
                                if (first && !string.IsNullOrEmpty(config.SystemSN))
                                {
                                    foreach (var text in run.Elements<Text>())
                                    {
                                        text.Text = config.SystemSN;
                                    }
                                    run.RunProperties.Highlight = null;
                                    first = false;
                                }
                                else
                                {
                                    run.Remove();
                                }
                            }
                            paragraphsProcessed.Add(paragraph);
                        }
                    }
                }
            }

            // Handle other yellow-highlighted replacements
            foreach (var run in body.Descendants<Run>())
            {
                var highlight = run.RunProperties?.Highlight;
                if (highlight != null && highlight.Val.HasValue && highlight.Val == HighlightColorValues.Yellow)
                {
                    // Skip if already processed as part of S/N
                    if (run.Parent is Paragraph p && paragraphsProcessed.Contains(p))
                        continue;

                    foreach (var text in run.Elements<Text>())
                    {
                        // Replace Contract "XXXX"
                        if (text.Text == "XXXX" && run.Parent is Paragraph para && para.InnerText.Contains("CONTRACT"))
                        {
                            text.Text = !string.IsNullOrEmpty(config.Contract) ? config.Contract : "____";
                        }
                        // Replace "VS/S/L/VL-XX" with actual variant and FOV (for title page)
                        // For ILET, exclude the FOV/Aperture part
                        else if (text.Text.Contains("VS/S/L/VL-XX"))
                        {
                            if (config.SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase))
                            {
                                // ILET: Show only variant (e.g., "ILET-6" instead of "ILET-6-6\"")
                                text.Text = text.Text.Replace("VS/S/L/VL-XX", config.METSVariant);
                            }
                            else
                            {
                                // METS or other: Show variant-FOV (e.g., "METS-S-16\"")
                                text.Text = text.Text.Replace("VS/S/L/VL-XX", $"{config.METSVariant}-{config.FOV}");
                            }
                        }
                        // Replace "VS/S/L/VL" standalone
                        else if (text.Text.Contains("VS/S/L/VL"))
                        {
                            text.Text = text.Text.Replace("VS/S/L/VL", config.METSVariant);
                        }
                        // Replace "XXXXX" placeholder (serial number suffix)
                        else if (text.Text == "XXXXX")
                        {
                            text.Text = "00001"; // Default serial number suffix
                        }
                        // Replace "XXXX" for other cases
                        else if (text.Text == "XXXX")
                        {
                            text.Text = "____"; // Blank for user to fill
                        }
                        // Replace "XX" for Exit Aperture or BB size
                        // Skip for ILET system name on title page and ATR page
                        else if (text.Text == "XX")
                        {
                            // Check if this is part of the system name (ILET-Variant-XX or METS-Variant-XX)
                            if (run.Parent is Paragraph para4)
                            {
                                string paraText3 = para4.InnerText;
                                bool isSystemNameContext = paraText3.Contains("Acceptance Test Procedure") ||
                                                          paraText3.Contains("ATP") ||
                                                          paraText3.Contains("Acceptance Test Results") ||
                                                          paraText3.Contains("ATR") ||
                                                          (paraText3.Contains(config.SystemType) && 
                                                           paraText3.Contains(config.METSVariant) &&
                                                           !paraText3.Contains("SR800N") && 
                                                           !paraText3.Contains("SR300N"));

                                // For ILET in system name context, don't add aperture
                                if (config.SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase) && isSystemNameContext)
                                {
                                    text.Text = ""; // Remove the XX placeholder for ILET system name
                                }
                                else
                                {
                                    text.Text = config.FOV; // Use FOV as default
                                }
                            }
                            else
                            {
                                text.Text = config.FOV; // Use FOV as default
                            }
                        }
                        // Replace "X" for BB size (SR800N-X)
                        else if (text.Text == "X" && run.Parent is Paragraph para3)
                        {
                            string paraText2 = para3.InnerText;
                            if (paraText2.Contains("SR800N-"))
                            {
                                text.Text = config.BBSize.Replace("D", ""); // e.g., "4D" ? "4"
                            }
                            else if (paraText2.Contains("SR300N-"))
                            {
                                text.Text = config.ISExitAperture.Replace("\"", ""); // e.g., "5\"" ? "5"
                            }
                        }
                    }

                    // Remove highlight after replacing
                    run.RunProperties.Highlight = null;
                }
            }
        }

        private void ReplaceRedTextPlaceholders(Body body, ATPConfiguration config, WordprocessingDocument wordDoc)
        {
            // Process main body
            ReplaceRedTextInPart(body, config);

            // Process all footers
            if (wordDoc.MainDocumentPart?.FooterParts != null)
            {
                foreach (var footerPart in wordDoc.MainDocumentPart.FooterParts)
                {
                    if (footerPart.Footer != null)
                    {
                        ReplaceRedTextInPart(footerPart.Footer, config);
                    }
                }
            }

            // Process all headers (if needed)
            if (wordDoc.MainDocumentPart?.HeaderParts != null)
            {
                foreach (var headerPart in wordDoc.MainDocumentPart.HeaderParts)
                {
                    if (headerPart.Header != null)
                    {
                        ReplaceRedTextInPart(headerPart.Header, config);
                    }
                }
            }
        }

        private void ReplaceRedTextInPart(OpenXmlElement element, ATPConfiguration config)
        {
            // Get all paragraphs to determine position in document (for title page detection)
            var allParagraphs = element.Descendants<Paragraph>().ToList();

            foreach (var run in element.Descendants<Run>())
            {
                var color = run.RunProperties?.Color;
                if (color != null && (color.Val == "FF0000" || color.Val == "EE0000" || color.Val == "Red" || color.Val == "C00000"))
                {
                    foreach (var text in run.Elements<Text>())
                    {
                        string originalText = text.Text;
                        string parentParaText = run.Parent is Paragraph p ? p.InnerText : "";

                        // Determine if we're on title page (first ~15 paragraphs) or ATR page
                        bool isOnTitlePage = false;
                        bool isOnATRPage = false;
                        if (run.Parent is Paragraph currentPara)
                        {
                            int paraIndex = allParagraphs.IndexOf(currentPara);
                            if (paraIndex >= 0 && paraIndex < 15)
                            {
                                isOnTitlePage = true;
                            }

                            // Check if we're on ATR page by looking at surrounding paragraphs
                            for (int i = Math.Max(0, paraIndex - 5); i < Math.Min(allParagraphs.Count, paraIndex + 5); i++)
                            {
                                string nearbyText = allParagraphs[i].InnerText;
                                if (nearbyText.Contains("Acceptance Test Results") || nearbyText.Contains("ATR"))
                                {
                                    isOnATRPage = true;
                                    break;
                                }
                            }
                        }

                        // Check if we're in a table cell context and get the entire row context
                        bool inTableCell = run.Ancestors<TableCell>().Any();
                        string tableCellContext = "";
                        string tableRowContext = "";
                        if (inTableCell)
                        {
                            var tableCell = run.Ancestors<TableCell>().FirstOrDefault();
                            if (tableCell != null)
                            {
                                tableCellContext = tableCell.InnerText;

                                // Get the entire row context
                                var tableRow = tableCell.Parent as TableRow;
                                if (tableRow != null)
                                {
                                    tableRowContext = tableRow.InnerText;
                                }
                            }
                        }
                        
                        // Debug: Log what we're processing
                        System.Diagnostics.Debug.WriteLine($"Processing: '{originalText}' | Para: '{parentParaText}' | Row: '{tableRowContext}'");
                        
                        // Pages 1, 3, 5, 8 - Apply user inputs to red-painted locations
                        // System S/N replacement
                        if (originalText.Contains("S/N") || originalText.Contains("Serial") || 
                            originalText == "26METS-S-00001-01" || originalText.Contains("00001"))
                        {
                            if (!string.IsNullOrEmpty(config.SystemSN))
                                text.Text = config.SystemSN;
                        }
                        // Contract replacement
                        else if ((originalText.Contains("D123456") || originalText.Contains("Contract:") || 
                                 originalText == "Contract") && !parentParaText.Contains("S/N"))
                        {
                            if (!string.IsNullOrEmpty(config.Contract))
                                text.Text = config.Contract;
                        }
                        // PM Name replacement
                        else if (originalText.Contains("PM Name") || originalText.Equals("PM Name", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(config.PMName))
                                text.Text = config.PMName;
                        }
                        // Order Number replacement (for footer: "DOrder#1000010" should become "D" + actual order number)
                        else if (originalText.Contains("Order#") || originalText.Contains("DOrder#") || originalText == "1000010")
                        {
                            if (!string.IsNullOrEmpty(config.OrderNumber))
                            {
                                // If the text contains "DOrder#", replace "Order#" with the actual order number
                                if (originalText.Contains("DOrder#"))
                                {
                                    text.Text = "D" + config.OrderNumber;
                                }
                                else if (originalText.Contains("Order#"))
                                {
                                    text.Text = originalText.Replace("Order#", config.OrderNumber).Replace("1000010", config.OrderNumber);
                                }
                                else
                                {
                                    text.Text = config.OrderNumber;
                                }
                            }
                        }
                        // System Type replacement
                        else if (originalText == "METS" || originalText == "ILET" || originalText == "WFOV" || originalText == "Type")
                        {
                            text.Text = config.SystemType;

                            // Make the Type value BOLD
                            if (run.RunProperties == null)
                            {
                                run.RunProperties = new RunProperties();
                            }

                            var bold = run.RunProperties.Elements<Bold>().FirstOrDefault();
                            if (bold == null)
                            {
                                run.RunProperties.AppendChild(new Bold());
                            }
                        }
                        // PRIORITY 1: Check for variant+mm patterns (Smm, Lmm, VSmm, VLmm, 4mm, 5mm, 6mm, Variantmm)
                        // Replace entire text with distance value + mm (case-insensitive)
                        else if (originalText.Equals("VSmm", StringComparison.OrdinalIgnoreCase) || 
                                 originalText.Equals("Smm", StringComparison.OrdinalIgnoreCase) || 
                                 originalText.Equals("Lmm", StringComparison.OrdinalIgnoreCase) || 
                                 originalText.Equals("VLmm", StringComparison.OrdinalIgnoreCase) ||
                                 originalText.Equals("4mm", StringComparison.OrdinalIgnoreCase) || 
                                 originalText.Equals("5mm", StringComparison.OrdinalIgnoreCase) || 
                                 originalText.Equals("6mm", StringComparison.OrdinalIgnoreCase) ||
                                 originalText.Equals("Variantmm", StringComparison.OrdinalIgnoreCase))
                        {
                            string variantDistance = config.GetVariantDistance();
                            if (!string.IsNullOrEmpty(variantDistance))
                            {
                                text.Text = variantDistance + "mm";
                                System.Diagnostics.Debug.WriteLine($"  -> Replaced with: {text.Text} (PRIORITY 1)");
                            }
                        }
                        // Also handle cases where "Smm" appears in focal length context (case-sensitive)
                        else if (originalText.EndsWith("mm", StringComparison.OrdinalIgnoreCase) && originalText.Length > 2 &&
                                 (parentParaText.Contains("focal length") || parentParaText.Contains("Cycles/mRad") || 
                                  parentParaText.Contains("Exit Aperture") || tableRowContext.Contains("Exit Aperture")))
                        {
                            // Extract the variant letter(s) before "mm"
                            string prefix = originalText.Substring(0, originalText.Length - 2);
                            // Check if this is a valid variant identifier (case-insensitive)
                            if (prefix.Equals("VS", StringComparison.OrdinalIgnoreCase) || 
                                prefix.Equals("S", StringComparison.OrdinalIgnoreCase) || 
                                prefix.Equals("L", StringComparison.OrdinalIgnoreCase) || 
                                prefix.Equals("VL", StringComparison.OrdinalIgnoreCase) ||
                                prefix == "4" || prefix == "5" || prefix == "6")
                            {
                                string variantDistance = config.GetVariantDistance();
                                if (!string.IsNullOrEmpty(variantDistance))
                                {
                                    text.Text = variantDistance + "mm";
                                    System.Diagnostics.Debug.WriteLine($"  -> Replaced with: {text.Text} (Context mm)");
                                }
                            }
                        }
                        // PRIORITY 2: Check if this is focal length/Cycles context - replace with distance value
                        else if ((parentParaText.Contains("focal length") || parentParaText.Contains("Cycles/mRad") || 
                                  parentParaText.Contains("cycles/mrad") || parentParaText.Contains("mRad units") ||
                                  parentParaText.Contains("calculations are based") ||
                                  tableRowContext.Contains("focal length") || tableRowContext.Contains("Cycles/mRad") ||
                                  tableRowContext.Contains("mRad units")) &&
                                 (originalText == "VS" || originalText == "S" || originalText == "L" || originalText == "VL" ||
                                  originalText == "4" || originalText == "5" || originalText == "6" || 
                                  originalText.Equals("Variant", StringComparison.OrdinalIgnoreCase)))
                        {
                            string variantDistance = config.GetVariantDistance();
                            if (!string.IsNullOrEmpty(variantDistance))
                            {
                                text.Text = variantDistance;
                                System.Diagnostics.Debug.WriteLine($"  -> Replaced with: {text.Text} (PRIORITY 2 - focal length)");
                            }
                        }
                        // PRIORITY 3: Check if this is Exit Aperture Diameter context - replace with distance value
                        else if ((parentParaText.Contains("Exit Aperture") || tableRowContext.Contains("Exit Aperture")) &&
                                 (originalText == "VS" || originalText == "S" || originalText == "L" || originalText == "VL" ||
                                  originalText == "4" || originalText == "5" || originalText == "6" || 
                                  originalText.Equals("Variant", StringComparison.OrdinalIgnoreCase)))
                        {
                            string variantDistance = config.GetVariantDistance();
                            if (!string.IsNullOrEmpty(variantDistance))
                            {
                                text.Text = variantDistance;
                                System.Diagnostics.Debug.WriteLine($"  -> Replaced with: {text.Text} (PRIORITY 3 - Exit Aperture)");
                            }
                        }
                        // PRIORITY 4: Variant replacement for system names (VS/S/L/VL for METS, or 4/5/6 for ILET)
                        // This catches "Variant" on pages 1 & 8 where it should show the variant NAME (e.g., "S", "L")
                        // Pages 9 & 10 are already caught by PRIORITY 2 & 3 above
                        else if (originalText == "VS" || originalText == "L" || originalText == "VL" ||
                                 originalText.Equals("Variant", StringComparison.OrdinalIgnoreCase) ||
                                 originalText.Contains("VS/S/L/VL"))
                        {
                            // For ILET on title page (page 1) or ATR page (page 9), hide variant in system name
                            bool isTitleOrATRContext = isOnTitlePage || isOnATRPage ||
                                                      parentParaText.Contains("Acceptance Test Procedure") || 
                                                      parentParaText.Contains("ATP") ||
                                                      parentParaText.Contains("For") ||
                                                      parentParaText.Contains("Acceptance Test Results") ||
                                                      parentParaText.Contains("ATR") ||
                                                      tableRowContext.Contains("System:") ||
                                                      tableCellContext.Contains("System:");

                            if (config.SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase) && isTitleOrATRContext)
                            {
                                text.Text = ""; // Hide variant for ILET on title/ATR pages

                                // Also remove the next hyphen if found
                                RemoveNextHyphen(run);
                            }
                            else
                            {
                                text.Text = config.METSVariant;
                                System.Diagnostics.Debug.WriteLine($"  -> Replaced with variant name: {text.Text} (PRIORITY 4)");
                            }
                        }
                        // Handle ILET numeric variants (4, 5, 6) on title and ATR pages
                        else if ((originalText == "4" || originalText == "5" || originalText == "6") &&
                                 !parentParaText.Contains("Table") && !parentParaText.Contains("focal length") &&
                                 !parentParaText.Contains("Cycles/mRad") && !parentParaText.Contains("Exit Aperture"))
                        {
                            // For ILET on title page (page 1) or ATR page (page 9), hide variant in system name
                            bool isTitleOrATRContext = isOnTitlePage || isOnATRPage ||
                                                      parentParaText.Contains("Acceptance Test Procedure") || 
                                                      parentParaText.Contains("ATP") ||
                                                      parentParaText.Contains("For") ||
                                                      parentParaText.Contains("Acceptance Test Results") ||
                                                      parentParaText.Contains("ATR") ||
                                                      tableRowContext.Contains("System:") ||
                                                      tableCellContext.Contains("System:");

                            if (config.SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase) && isTitleOrATRContext)
                            {
                                text.Text = ""; // Hide variant for ILET on title/ATR pages
                                System.Diagnostics.Debug.WriteLine($"  -> Hid ILET variant: {originalText} (Title/ATR context)");

                                // Also remove the next hyphen if found
                                RemoveNextHyphen(run);
                            }
                            else
                            {
                                text.Text = config.METSVariant;
                            }
                        }
                        // Special handling for "S" - only replace with variant name if NOT in focal length context
                        else if (originalText == "S" && !parentParaText.Contains("S/N") && !parentParaText.Contains("System"))
                        {
                            // If we get here, it's not in focal length context (would have been caught above)
                            // For ILET on title page (page 1) or ATR page (page 9), hide variant in system name
                            bool isTitleOrATRContext = isOnTitlePage || isOnATRPage ||
                                                      parentParaText.Contains("Acceptance Test Procedure") || 
                                                      parentParaText.Contains("ATP") ||
                                                      parentParaText.Contains("For") ||
                                                      parentParaText.Contains("Acceptance Test Results") ||
                                                      parentParaText.Contains("ATR") ||
                                                      tableRowContext.Contains("System:") ||
                                                      tableCellContext.Contains("System:");

                            if (config.SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase) && isTitleOrATRContext)
                            {
                                text.Text = ""; // Hide variant for ILET on title/ATR pages

                                // Also remove the next hyphen if found
                                RemoveNextHyphen(run);
                            }
                            else
                            {
                                text.Text = config.METSVariant;
                            }
                        }
                        // FOV replacement on first page
                        else if (originalText == "FOV")
                        {
                            // Skip FOV display for ILET on title page (where System: ILET-Variant should not include FOV)
                            bool isTitlePageContext = parentParaText.Contains("Acceptance Test Procedure") || 
                                                     parentParaText.Contains("ATP") ||
                                                     tableRowContext.Contains("System:");
                            bool isATRContext = parentParaText.Contains("Acceptance Test Results") ||
                                               parentParaText.Contains("ATR") ||
                                               tableRowContext.Contains("Acceptance Test Results");

                            if (config.SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase) && 
                                (isTitlePageContext || isATRContext))
                            {
                                // For ILET on title/ATR pages, don't show FOV in system name
                                text.Text = "";
                            }
                            else
                            {
                                text.Text = config.FOV + "\"";
                            }
                        }
                        // Page 9 & 10 - Variant distance replacement based on system type
                        else if (originalText == "1023.3" || originalText == "1787.1" || originalText == "3048" || 
                                 originalText == "762" || originalText == "1016")
                        {
                            string variantDistance = config.GetVariantDistance();
                            if (!string.IsNullOrEmpty(variantDistance))
                                text.Text = variantDistance;
                        }
                        // FOV replacement - match any aperture format like "6\"", "10\"", "16\"", etc.
                        else if (originalText.Contains("\"") && 
                                 (originalText.Contains("10") || originalText.Contains("12") || 
                                  originalText.Contains("14") || originalText.Contains("16") || originalText.Contains("19") || 
                                  originalText.Contains("21") || originalText.Contains("15") || originalText.Contains("30") || 
                                  originalText.Contains("40") || originalText == "2\"" || originalText == "3\"" || 
                                  originalText == "4\"" || originalText == "5\"" || originalText == "6\"" || 
                                  originalText == "7\"" || originalText == "8\"" || originalText == "9\""))
                        {
                            // Skip FOV display for ILET on title page and ATR page
                            bool isTitlePageContext = parentParaText.Contains("Acceptance Test Procedure") || 
                                                     parentParaText.Contains("ATP") ||
                                                     parentParaText.Contains("For") ||
                                                     (tableRowContext.Contains("System:") && !tableRowContext.Contains("Targets"));
                            bool isATRContext = parentParaText.Contains("Acceptance Test Results") ||
                                               parentParaText.Contains("ATR") ||
                                               tableRowContext.Contains("Acceptance Test Results") ||
                                               tableRowContext.Contains("System:") && tableCellContext.Contains("System:");

                            if (config.SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase) && 
                                (isTitlePageContext || isATRContext))
                            {
                                // For ILET on title/ATR pages, don't show FOV in system name - remove it
                                text.Text = "";
                            }
                            else if (!string.IsNullOrEmpty(config.FOV))
                            {
                                text.Text = config.FOV + "\"";
                            }
                        }
                        // Blackbody Size replacement (XX in SR800N-XX)
                        else if (originalText == "XX" && run.Parent is Paragraph para && para.InnerText.Contains("SR800N"))
                        {
                            if (!string.IsNullOrEmpty(config.BBSize))
                                text.Text = config.BBSize;
                        }
                        // Integrating Sphere Exit Aperture replacement (X in SR300N-X)
                        else if (originalText == "X" && run.Parent is Paragraph para2 && para2.InnerText.Contains("SR300N"))
                        {
                            if (!string.IsNullOrEmpty(config.ISExitAperture))
                                text.Text = config.ISExitAperture.Replace("\"", "");
                        }
                        // Focus Stage Finite Distance replacement
                        else if (text.Text == "A" || text.Text == "B")
                        {
                            if (!string.IsNullOrEmpty(config.FocusStageFiniteDistance))
                                text.Text = config.FocusStageFiniteDistance;
                        }
                        // Aperture replacement - convert inches to mm (for Table 2: Visual Inspection Results Table)
                        else if (originalText == "Aperture" || originalText.Equals("Aperture", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(config.FOV))
                            {
                                // Convert FOV from inches to mm (1 inch = 25.4 mm)
                                string fovValue = config.FOV.Replace("\"", "").Trim();
                                if (double.TryParse(fovValue, out double inches))
                                {
                                    double mm = inches * 25.4;
                                    text.Text = mm.ToString("0.0"); // Format with 1 decimal place
                                }
                            }

                            // Remove bold formatting if present
                            if (run.RunProperties != null)
                            {
                                var bold = run.RunProperties.Elements<Bold>().FirstOrDefault();
                                if (bold != null)
                                {
                                    bold.Remove();
                                }
                            }
                        }
                        // Keep unit markers like "m", "[nm]", etc.
                        else if (text.Text == "m" || text.Text == "[m]" || text.Text == "[nm]")
                        {
                            // Keep as-is
                        }
                    }

                    // Change red text to black after replacing
                    run.RunProperties.Color.Val = "000000";
                }
            }

            // Clean up trailing hyphens for ILET system names (e.g., "ILET-6-" -> "ILET-6")
            if (config.SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase))
            {
                CleanUpILETSystemNames(element, config);
            }
        }

        private void RemoveNextHyphen(Run currentRun)
        {
            // Look for the next hyphen in the same paragraph and remove it
            var paragraph = currentRun.Parent as Paragraph;
            if (paragraph == null) return;

            var runs = paragraph.Elements<Run>().ToList();
            int currentIndex = runs.IndexOf(currentRun);

            if (currentIndex < 0) return;

            // Look at the next few runs to find a hyphen
            for (int i = currentIndex + 1; i < Math.Min(currentIndex + 5, runs.Count); i++)
            {
                var nextRun = runs[i];
                bool foundHyphen = false;

                foreach (var text in nextRun.Elements<Text>().ToList())
                {
                    string textContent = text.Text;

                    // If this text is just a hyphen or starts with hyphen, remove it
                    if (textContent.Trim() == "-")
                    {
                        text.Text = "";
                        foundHyphen = true;
                        break;
                    }
                    else if (textContent.TrimStart().StartsWith("-"))
                    {
                        text.Text = textContent.TrimStart('-');
                        foundHyphen = true;
                        break;
                    }
                    else if (!string.IsNullOrWhiteSpace(textContent))
                    {
                        // Found non-hyphen, non-whitespace content, stop looking
                        return;
                    }
                }

                if (foundHyphen)
                {
                    // Successfully removed a hyphen, we're done
                    return;
                }
            }
        }

        private void CleanUpILETSystemNames(OpenXmlElement element, ATPConfiguration config)
        {
            // Find paragraphs that contain ILET system names on title page or ATR page
            foreach (var paragraph in element.Descendants<Paragraph>())
            {
                string paraText = paragraph.InnerText.Trim();

                // Check if this is title page or ATR page context and contains ILET
                bool isRelevantContext = (paraText.Contains("Acceptance Test Procedure") ||
                                         paraText.Contains("ATP") ||
                                         paraText.Contains("For") ||
                                         paraText.Contains("Acceptance Test Results") ||
                                         paraText.Contains("ATR")) && paraText.Contains("ILET");

                if (!isRelevantContext) continue;

                // Get all runs in this paragraph
                var runs = paragraph.Elements<Run>().ToList();

                // Look for consecutive hyphens and remove duplicates
                for (int i = 0; i < runs.Count - 1; i++)
                {
                    var currentRun = runs[i];
                    var currentText = currentRun.InnerText.Trim();

                    // If current run is a hyphen or ends with hyphen
                    if (currentText == "-" || currentText.EndsWith("-"))
                    {
                        // Look at the next few runs to find another hyphen
                        for (int j = i + 1; j < Math.Min(i + 4, runs.Count); j++)
                        {
                            var nextRun = runs[j];
                            var nextText = nextRun.InnerText.Trim();

                            // Skip empty runs
                            if (string.IsNullOrWhiteSpace(nextText))
                                continue;

                            // If we found another hyphen, remove it
                            if (nextText == "-" || nextText.StartsWith("-"))
                            {
                                // Remove the hyphen from this run
                                foreach (var text in nextRun.Elements<Text>())
                                {
                                    if (text.Text.Trim() == "-")
                                    {
                                        text.Text = "";
                                    }
                                    else if (text.Text.StartsWith("-"))
                                    {
                                        text.Text = text.Text.TrimStart('-');
                                    }
                                }
                                break; // Found and removed one hyphen, move on
                            }
                            else
                            {
                                // Found non-hyphen content, stop looking
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void FilterMajorComponentsList(Body body, ATPConfiguration config)
        {
            // Find the Major Components section
            Paragraph? majorComponentsHeader = null;
            Paragraph? lastComponentPara = null;
            ParagraphProperties? existingBulletStyle = null;
            bool inMajorComponents = false;

            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                string paraText = paragraph.InnerText.Trim();

                // Detect Major Components section
                if (paraText.Contains("Major Components"))
                {
                    majorComponentsHeader = paragraph;
                    inMajorComponents = true;
                    continue;
                }

                // Stop at next major section
                if (inMajorComponents && (paraText.StartsWith("3.") || paraText.Contains("Test Procedures")))
                {
                    inMajorComponents = false;
                    break;
                }

                // Track the last component in the list and capture its style
                if (inMajorComponents && !string.IsNullOrWhiteSpace(paraText))
                {
                    lastComponentPara = paragraph;
                    // Capture the bullet style from an existing item
                    if (existingBulletStyle == null && paragraph.ParagraphProperties != null)
                    {
                        existingBulletStyle = (ParagraphProperties)paragraph.ParagraphProperties.CloneNode(true);
                    }
                }
            }

            // Add radiation sources to Major Components list (Page 4)
            if (lastComponentPara != null && lastComponentPara.Parent != null)
            {
                OpenXmlElement insertAfter = lastComponentPara;

                // Add B.B if selected
                if (config.HasBB && !string.IsNullOrEmpty(config.BBSize))
                {
                    Paragraph bbPara = CreateBulletParagraph($"IR Source: SR800N-{config.BBSize}-RR Blackbody + Controller.", existingBulletStyle);
                    lastComponentPara.Parent.InsertAfter(bbPara, insertAfter);
                    insertAfter = bbPara;
                }

                // Add I.S if selected
                if (config.HasIS && !string.IsNullOrEmpty(config.ISExitAperture))
                {
                    string aperture = config.ISExitAperture.Replace("\"", "");
                    Paragraph isPara = CreateBulletParagraph($"VIS Source: SR300N-{aperture}\" integrating sphere + Controller.", existingBulletStyle);
                    lastComponentPara.Parent.InsertAfter(isPara, insertAfter);
                    insertAfter = isPara;
                }

                // Add LOS Laser if selected
                if (config.HasLOSLaser)
                {
                    Paragraph losPara = CreateBulletParagraph("LOS laser diode.", existingBulletStyle);
                    lastComponentPara.Parent.InsertAfter(losPara, insertAfter);
                    insertAfter = losPara;
                }
            }
        }

        private Paragraph CreateBulletParagraph(string text, ParagraphProperties? styleTemplate)
        {
            Paragraph para = new Paragraph();
            
            if (styleTemplate != null)
            {
                // Clone the existing bullet style
                ParagraphProperties paraProps = (ParagraphProperties)styleTemplate.CloneNode(true);
                para.Append(paraProps);
            }
            else
            {
                // Fallback: create basic bullet style
                ParagraphProperties paraProps = new ParagraphProperties();
                NumberingProperties numProps = new NumberingProperties();
                numProps.Append(new NumberingLevelReference() { Val = 0 });
                numProps.Append(new NumberingId() { Val = 1 });
                paraProps.Append(numProps);
                para.Append(paraProps);
            }
            
            Run run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);
            
            return para;
        }

        private void CreateNewATPDocument(string filePath, ATPConfiguration config)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Add title
                Paragraph titlePara = body.AppendChild(new Paragraph());
                Run titleRun = titlePara.AppendChild(new Run());
                RunProperties titleProps = titleRun.AppendChild(new RunProperties());
                titleProps.AppendChild(new Bold());
                titleProps.AppendChild(new FontSize() { Val = "32" });
                titleRun.AppendChild(new Text("Acceptance Test Procedure (ATP)") { Space = SpaceProcessingModeValues.Preserve });

                // Add empty line
                body.AppendChild(new Paragraph());

                // Add System Configuration section
                AddHeading(body, "System Configuration");
                AddParagraph(body, $"System Type: {config.SystemType}");
                
                if (!string.IsNullOrEmpty(config.METSVariant))
                    AddParagraph(body, $"METS Variant: {config.METSVariant}");
                
                AddParagraph(body, $"Field of View (FOV): {config.FOV}");

                // Add empty line
                body.AppendChild(new Paragraph());

                // Add System Components section
                AddHeading(body, "System Components");
                
                if (config.HasSourceStage)
                    AddBulletPoint(body, "Source Stage");
                
                if (config.HasTargetWheel)
                {
                    AddBulletPoint(body, "Target Wheel");
                    if (!string.IsNullOrEmpty(config.TargetWheelPDFPath))
                        AddParagraph(body, $"  Target Wheel Scheme: {Path.GetFileName(config.TargetWheelPDFPath)}");
                }
                
                if (config.HasFocusStage)
                    AddBulletPoint(body, "Focus Stage");

                // Add empty lines
                body.AppendChild(new Paragraph());
                body.AppendChild(new Paragraph());

                // Add Test Procedures section
                AddHeading(body, "Test Procedures");
                AddParagraph(body, "1. Visual Inspection");
                AddParagraph(body, "2. Power-On Test");
                AddParagraph(body, "3. Functional Tests");
                AddParagraph(body, "4. Performance Verification");
                AddParagraph(body, "5. Final Acceptance");

                // Add empty lines
                body.AppendChild(new Paragraph());
                body.AppendChild(new Paragraph());

                // Add footer
                AddParagraph(body, $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                // Update all fields (including Table of Contents) when document is opened
                UpdateFieldsOnOpen(wordDoc);

                mainPart.Document.Save();
            }
        }

        private void AddHeading(Body body, string text)
        {
            Paragraph para = body.AppendChild(new Paragraph());
            Run run = para.AppendChild(new Run());
            RunProperties runProps = run.AppendChild(new RunProperties());
            runProps.AppendChild(new Bold());
            runProps.AppendChild(new FontSize() { Val = "28" });
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        }

        private void AddParagraph(Body body, string text)
        {
            Paragraph para = body.AppendChild(new Paragraph());
            Run run = para.AppendChild(new Run());
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        }

        private void AddBulletPoint(Body body, string text)
        {
            Paragraph para = body.AppendChild(new Paragraph());
            
            ParagraphProperties paraProps = para.AppendChild(new ParagraphProperties());
            NumberingProperties numProps = paraProps.AppendChild(new NumberingProperties());
            numProps.AppendChild(new NumberingLevelReference() { Val = 0 });
            numProps.AppendChild(new NumberingId() { Val = 1 });

            Run run = para.AppendChild(new Run());
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        }

        private void UpdateATRTable(Body body, ATPConfiguration config)
        {
            System.Diagnostics.Debug.WriteLine("=== UpdateATRTable START ===");
            System.Diagnostics.Debug.WriteLine($"_componentSections is null: {_componentSections == null}");
            System.Diagnostics.Debug.WriteLine($"_componentSections count: {_componentSections?.Count ?? 0}");
            
            if (_componentSections != null)
            {
                foreach (var sec in _componentSections)
                {
                    System.Diagnostics.Debug.WriteLine($"  Section: {sec.ListOfTestsEntry}");
                }
            }
            
            // Find the ATR (ACCEPTANCE TEST RESULTS) table on page 9
            // Look for "Table 1: Results Table"
            Table? atrTable = null;
            
            foreach (var table in body.Descendants<Table>())
            {
                string tableText = table.InnerText;
                // Look for the Results table which has "Table 1: Results Table" or columns like "Test Name", "Paragraph", "Results", etc.
                if ((tableText.Contains("Results Table") || tableText.Contains("Test name")) && 
                    tableText.Contains("Paragraph") && tableText.Contains("Results"))
                {
                    atrTable = table;
                    System.Diagnostics.Debug.WriteLine("Found ATR table");
                    break;
                }
            }

            if (atrTable == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Could not find Results Table (ATR table)");
                return;
            }

            System.Diagnostics.Debug.WriteLine("ATR Table rows:");
            int rowIndex = 0;
            // Find the row with marker text - could be "Component name" or "Start Adding here"
            TableRow? markerRow = null;
            TableRow? templateRow = null; // Use the previous row as a template for formatting
            
            foreach (var row in atrTable.Elements<TableRow>())
            {
                string rowText = row.InnerText.Trim();
                System.Diagnostics.Debug.WriteLine($"  Row {rowIndex}: {rowText}");
                
                // Look for the marker row - can be "Component name", "Start Adding here", or similar placeholder
                if ((rowText.Contains("Component name") && rowText.Contains("Paragraph")) ||
                    rowText.Contains("Start Adding here") ||
                    rowText.Contains("Start adding here"))
                {
                    markerRow = row;
                    // Get the previous row as a template for cell formatting
                    templateRow = row.PreviousSibling<TableRow>();
                    System.Diagnostics.Debug.WriteLine($"  >>> FOUND MARKER ROW at index {rowIndex}!");
                    System.Diagnostics.Debug.WriteLine($"  >>> Template row: {templateRow?.InnerText}");
                    break;
                }
                rowIndex++;
            }

            if (markerRow == null)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Could not find marker row in ATR table");
                return;
            }

            // Use the stored component sections from AddComponentTestSections
            if (_componentSections == null || _componentSections.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No component sections available - removing marker row");
                markerRow.Remove();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Total component sections to process: {_componentSections.Count}");

            // Start numbering from 10 and use section 3.11, 3.12, etc. (3.10 is Radiometric Offset Test)
            int testNumber = 10;
            int sectionNumber = 11;
            bool firstComponent = true;
            TableRow? currentInsertAfter = null;
            int componentsAdded = 0;

            foreach (var section in _componentSections)
            {
                System.Diagnostics.Debug.WriteLine($"Processing section: {section.ListOfTestsEntry}");
                
                // Skip Target Wheel as it's already in the template (row #8)
                if (section.ListOfTestsEntry.Contains("Target Wheel"))
                {
                    System.Diagnostics.Debug.WriteLine("  -> Skipping Target Wheel (already in template)");
                    continue;
                }

                System.Diagnostics.Debug.WriteLine($"  -> Creating row #{testNumber}: {section.ListOfTestsEntry} (Section 3.{sectionNumber})");

                // Create new row with proper formatting from template
                TableRow newRow = CreateATRTableRowFromTemplate(
                    templateRow,
                    testNumber.ToString(),
                    section.ListOfTestsEntry,
                    $"3.{sectionNumber}",
                    "O.K.", "", "" // Results = "O.K.", Pass/fail and Notes - empty
                );

                if (firstComponent)
                {
                    // Replace the marker row with the first component
                    System.Diagnostics.Debug.WriteLine($"  -> REPLACING marker row with first component: {section.ListOfTestsEntry}");
                    if (markerRow.Parent != null)
                    {
                        markerRow.Parent.InsertBefore(newRow, markerRow);
                        markerRow.Remove(); // Remove the marker row
                        currentInsertAfter = newRow;
                        firstComponent = false;
                        componentsAdded++;
                        System.Diagnostics.Debug.WriteLine($"  -> Successfully replaced marker row");
                    }
                }
                else
                {
                    // Insert after the current row
                    System.Diagnostics.Debug.WriteLine($"  -> ADDING component after previous: {section.ListOfTestsEntry}");
                    if (currentInsertAfter?.Parent != null)
                    {
                        currentInsertAfter.Parent.InsertAfter(newRow, currentInsertAfter);
                        currentInsertAfter = newRow;
                        componentsAdded++;
                        System.Diagnostics.Debug.WriteLine($"  -> Successfully added row");
                    }
                }

                testNumber++;
                sectionNumber++;
            }

            // If no components were added (only Target Wheel was selected), remove the marker row
            if (componentsAdded == 0 && markerRow.Parent != null)
            {
                System.Diagnostics.Debug.WriteLine("No components added (only Target Wheel selected), removing marker row");
                markerRow.Remove();
            }

            System.Diagnostics.Debug.WriteLine($"=== UpdateATRTable END: Added {componentsAdded} rows starting from test #10 ===");
        }
        
        private TableRow CreateATRTableRowFromTemplate(TableRow? templateRow, string testNo, string testName, string paragraph, string results, string passFail, string notes)
        {
            TableRow row = new TableRow();
            
            // If we have a template row, copy its properties
            if (templateRow != null && templateRow.TableRowProperties != null)
            {
                row.TableRowProperties = (TableRowProperties)templateRow.TableRowProperties.CloneNode(true);
            }

            // Get the template cells for formatting
            var templateCells = templateRow?.Elements<TableCell>().ToList();
            
            // Create cells with proper formatting
            string[] cellTexts = new[] { testNo, testName, paragraph, results, passFail, notes };
            
            for (int i = 0; i < cellTexts.Length; i++)
            {
                TableCell cell = new TableCell();
                
                // Copy cell properties from template if available
                if (templateCells != null && i < templateCells.Count && templateCells[i].TableCellProperties != null)
                {
                    cell.TableCellProperties = (TableCellProperties)templateCells[i].TableCellProperties.CloneNode(true);
                }
                
                // Create paragraph with text - LEFT aligned
                Paragraph para = new Paragraph();
                ParagraphProperties paraProps = new ParagraphProperties();
                paraProps.Append(new Justification() { Val = JustificationValues.Left });
                paraProps.Append(new BiDi() { Val = new OnOffValue(false) }); // Force LTR direction
                para.Append(paraProps);
                
                Run run = new Run(new Text(cellTexts[i]) { Space = SpaceProcessingModeValues.Preserve });
                para.Append(run);
                cell.Append(para);
                
                row.Append(cell);
            }

            return row;
        }

        private TableRow CreateATRTableRow(string testNo, string testName, string paragraph, string results, string passFail, string notes)
        {
            TableRow row = new TableRow();

            // Copy table row properties from template if needed
            // Create cells matching the exact structure: # | Test name | Paragraph | Results | Pass/fail | Notes
            
            // Column 1: # (Test Number)
            TableCell cell1 = CreateTableCell(testNo);
            row.Append(cell1);

            // Column 2: Test name
            TableCell cell2 = CreateTableCell(testName);
            row.Append(cell2);

            // Column 3: Paragraph
            TableCell cell3 = CreateTableCell(paragraph);
            row.Append(cell3);

            // Column 4: Results (empty)
            TableCell cell4 = CreateTableCell(results);
            row.Append(cell4);

            // Column 5: Pass/fail (empty)
            TableCell cell5 = CreateTableCell(passFail);
            row.Append(cell5);

            // Column 6: Notes (empty)
            TableCell cell6 = CreateTableCell(notes);
            row.Append(cell6);

            return row;
        }
        
        private TableCell CreateTableCell(string text)
        {
            TableCell cell = new TableCell();
            Paragraph para = new Paragraph();
            ParagraphProperties paraProps = new ParagraphProperties();
            paraProps.Append(new Justification() { Val = JustificationValues.Center });
            para.Append(paraProps);
            
            Run run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);
            cell.Append(para);
            
            return cell;
        }

        private void PopulateSystemTargetsTable(Body body, string csvFilePath)
        {
            try
            {
                // Parse CSV file
                var targetData = ParseTargetCSV(csvFilePath);

                if (targetData == null || targetData.Count == 0)
                {
                    return;
                }

                // Find Table 3: System Targets
                Table? systemTargetsTable = null;
                int tableCount = 0;
                
                foreach (var table in body.Descendants<Table>())
                {
                    tableCount++;
                    string tableText = table.InnerText;

                    // Look for "System Targets" OR "Table 3" OR tables containing "Target Name" + "Dimension" + "Located At Plate"
                    if (tableText.Contains("System Targets") || 
                        (tableText.Contains("Table 3") && tableText.Contains("Target")) ||
                        (tableText.Contains("Target Name") && tableText.Contains("Dimension") && tableText.Contains("Located At Plate")) ||
                        (tableText.Contains("Target Name") && tableText.Contains("Dimension (mm)")) ||
                        (tableText.Contains("Column A from CSV") && tableText.Contains("Column C from CSV")))
                    {
                        systemTargetsTable = table;
                        break;
                    }
                }

                if (systemTargetsTable == null)
                {
                    return;
                }

                // Find header row and save template row
                TableRow? headerRow = null;
                TableRow? templateRow = null;
                var allRows = systemTargetsTable.Elements<TableRow>().ToList();

                for (int i = 0; i < allRows.Count; i++)
                {
                    string rowText = allRows[i].InnerText.Trim();

                    // Find header row (contains "No." and "Target Name")
                    if (rowText.Contains("No.") && rowText.Contains("Target Name"))
                    {
                        headerRow = allRows[i];

                        // Use the next row as template
                        if (i + 1 < allRows.Count)
                        {
                            templateRow = allRows[i + 1];
                        }
                        break;
                    }
                }

                if (headerRow == null)
                {
                    return;
                }

                // Remove ALL data rows (everything after header)
                var rowsToRemove = new List<TableRow>();
                bool foundHeader = false;
                
                foreach (var row in systemTargetsTable.Elements<TableRow>())
                {
                    if (row == headerRow)
                    {
                        foundHeader = true;
                        continue; // Keep header
                    }
                    
                    if (foundHeader)
                    {
                        rowsToRemove.Add(row);
                    }
                }

                foreach (var row in rowsToRemove)
                {
                    row.Remove();
                }

                // Now add new rows from CSV data
                TableRow? lastInsertedRow = headerRow;
                int targetNumber = 1;

                foreach (var target in targetData)
                {
                    TableRow newRow = CreateSystemTargetRow(
                        templateRow,
                        targetNumber.ToString(),
                        target.Name,
                        target.Size,
                        target.PlateNumber
                    );

                    // Insert after the last inserted row
                    if (lastInsertedRow?.Parent != null)
                    {
                        lastInsertedRow.Parent.InsertAfter(newRow, lastInsertedRow);
                        lastInsertedRow = newRow;
                    }

                    targetNumber++;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void CleanSystemTargetsTable(Body body)
        {
            try
            {
                // Find Table 3: System Targets
                Table? systemTargetsTable = null;

                foreach (var table in body.Descendants<Table>())
                {
                    string tableText = table.InnerText;

                    // Look for "System Targets" OR "Table 3" OR tables containing "Target Name" + "Dimension" + "Located At Plate"
                    if (tableText.Contains("System Targets") || 
                        (tableText.Contains("Table 3") && tableText.Contains("Target")) ||
                        (tableText.Contains("Target Name") && tableText.Contains("Dimension") && tableText.Contains("Located At Plate")) ||
                        (tableText.Contains("Target Name") && tableText.Contains("Dimension (mm)")) ||
                        (tableText.Contains("Column A from CSV") && tableText.Contains("Column C from CSV")))
                    {
                        systemTargetsTable = table;
                        break;
                    }
                }

                if (systemTargetsTable == null)
                {
                    return;
                }

                // Find header row
                TableRow? headerRow = null;

                foreach (var row in systemTargetsTable.Elements<TableRow>())
                {
                    string rowText = row.InnerText.Trim();

                    // Find header row (contains "No." and "Target Name")
                    if (rowText.Contains("No.") && rowText.Contains("Target Name"))
                    {
                        headerRow = row;
                        break;
                    }
                }

                if (headerRow == null)
                {
                    return;
                }

                // Clear all text from data rows (keep structure but empty the content)
                bool foundHeader = false;

                foreach (var row in systemTargetsTable.Elements<TableRow>())
                {
                    string rowText = row.InnerText.Trim();

                    // Skip the row that contains "Table 3: System Targets" title
                    if (rowText.Contains("Table 3:") && rowText.Contains("System Targets"))
                    {
                        continue; // Skip title row
                    }

                    if (row == headerRow)
                    {
                        foundHeader = true;
                        continue; // Skip header
                    }

                    if (foundHeader)
                    {
                        // Clear text from all cells in this row (including template text like "Column A from CSV")
                        foreach (var cell in row.Elements<TableCell>())
                        {
                            // Remove ALL children (paragraphs, tables, everything)
                            cell.RemoveAllChildren();

                            // Add a single empty paragraph to maintain cell structure
                            Paragraph emptyPara = new Paragraph();
                            Run emptyRun = new Run();
                            Text emptyText = new Text("");
                            emptyText.Space = SpaceProcessingModeValues.Preserve;
                            emptyRun.Append(emptyText);
                            emptyPara.Append(emptyRun);
                            cell.Append(emptyPara);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in CleanSystemTargetsTable: {ex.Message}");
            }
        }

        private List<TargetData> ParseTargetCSV(string csvFilePath)
        {
            var targets = new List<TargetData>();

            try
            {
                if (!File.Exists(csvFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: CSV file not found: {csvFilePath}");
                    return targets;
                }

                string[] lines = File.ReadAllLines(csvFilePath);
                
                // Skip header row (row 0) and start from row 1
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    // Split by comma
                    string[] columns = line.Split(',');
                    
                    if (columns.Length < 4)
                    {
                        System.Diagnostics.Debug.WriteLine($"WARNING: Skipping invalid row {i}: {line}");
                        continue;
                    }

                    var target = new TargetData
                    {
                        Name = columns[0].Trim(),           // Column A: Name
                        Type = columns[1].Trim(),           // Column B: type
                        Size = columns[2].Trim(),           // Column C: size
                        PlateNumber = columns[3].Trim(),    // Column D: plate #
                        AbsPosition = columns.Length > 4 ? columns[4].Trim() : "",  // Column E: abs. position
                        TempOffset = columns.Length > 5 ? columns[5].Trim() : ""    // Column F: temp.offset
                    };

                    targets.Add(target);
                    System.Diagnostics.Debug.WriteLine($"Parsed target {targets.Count}: {target.Name}, Size: {target.Size}, Plate: {target.PlateNumber}");
                }

                System.Diagnostics.Debug.WriteLine($"Total targets parsed: {targets.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR parsing CSV: {ex.Message}");
            }

            return targets;
        }

        private TableRow CreateSystemTargetRow(TableRow? templateRow, string no, string targetName, string dimension, string plateNumber)
        {
            TableRow row = new TableRow();

            // Copy row properties from template if available
            if (templateRow != null && templateRow.TableRowProperties != null)
            {
                row.TableRowProperties = (TableRowProperties)templateRow.TableRowProperties.CloneNode(true);
            }

            // Get template cells for formatting
            var templateCells = templateRow?.Elements<TableCell>().ToList();

            // Format dimension to 3 decimal places
            string formattedDimension = dimension;
            if (double.TryParse(dimension, out double dimensionValue))
            {
                formattedDimension = dimensionValue.ToString("0.###");
            }

            // Create 5 cells: No. | Target Name | Dimension (mRad) | Located At Plate | O.K
            string[] cellTexts = new[] { no, targetName, formattedDimension, plateNumber, "" }; // Last cell (O.K) is empty
            
            for (int i = 0; i < cellTexts.Length; i++)
            {
                TableCell cell = new TableCell();
                
                // Copy cell properties from template if available
                if (templateCells != null && i < templateCells.Count && templateCells[i].TableCellProperties != null)
                {
                    cell.TableCellProperties = (TableCellProperties)templateCells[i].TableCellProperties.CloneNode(true);
                }
                else if (templateCells != null && i < templateCells.Count)
                {
                    // Create basic cell properties if template doesn't have them
                    cell.TableCellProperties = new TableCellProperties();
                }
                
                // Create paragraph with text
                Paragraph para = new Paragraph();
                ParagraphProperties paraProps = new ParagraphProperties();
                
                // Column 1 (index 1) is "Target Name" - FORCE LEFT alignment
                if (i == 1)
                {
                    // Force left alignment at paragraph level
                    paraProps.Append(new Justification() { Val = JustificationValues.Left });
                    
                    // Also ensure BiDi is set to false (LTR, not RTL)
                    paraProps.Append(new BiDi() { Val = new OnOffValue(false) });
                }
                else
                {
                    // All other columns - CENTER aligned
                    paraProps.Append(new Justification() { Val = JustificationValues.Center });
                }
                
                para.Append(paraProps);
                
                // Create run with text
                Run run = new Run(new Text(cellTexts[i]) { Space = SpaceProcessingModeValues.Preserve });
                para.Append(run);
                cell.Append(para);
                
                row.Append(cell);
            }

            return row;
        }

        // Helper class to store target data from CSV
        private class TargetData
        {
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public string Size { get; set; } = "";
            public string PlateNumber { get; set; } = "";
            public string AbsPosition { get; set; } = "";
            public string TempOffset { get; set; } = "";
        }
    }

}
