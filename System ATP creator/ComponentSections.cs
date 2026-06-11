using System.Collections.Generic;

namespace System_ATP_creator
{
    /// <summary>
    /// Contains standardized ATP sections for each component
    /// Each section includes: Table of Contents entry, List of Tests entry, and Test Procedure text
    /// </summary>
    public static class ComponentSections
    {
        // Integrating Sphere (I.S) Section
        public static ComponentSection IntegratingSphere(string exitAperture)
        {
            return new ComponentSection
            {
                TableOfContentsEntry = $"VIS Source: SR300N-{exitAperture}\" integrating sphere + Controller",
                ListOfTestsEntry = $"SR300N-{exitAperture}\" integrating sphere",
                TestProcedureSection = $@"SR300N-{exitAperture}"" integrating sphere
Test Procedure: 
Verify that the ATP for the SR-300N-{exitAperture}"" is completed according to the procedure          described in doc. # D120000005.
Record the test results in the ATR document."
            };
        }

        // Blackbody (B.B) Section
        public static ComponentSection Blackbody(string type, string size)
        {
            // Different procedure for SR200N-33 vs RR/STD
            if (type.Equals("SR200N-33", StringComparison.OrdinalIgnoreCase))
            {
                return new ComponentSection
                {
                    TableOfContentsEntry = "SR200N-33 Blackbody",
                    ListOfTestsEntry = "IR Source: SR-200N-33",
                    TestProcedureSection = @"SR200N-33 Blackbody
Test Procedure:
Verify that the ATR for the SR-200N-33 is completed according to the procedure described in doc. # TP7241000010 & TP7241000020."
                };
            }
            else
            {
                // For RR or STD types
                return new ComponentSection
                {
                    TableOfContentsEntry = $"IR Source: SR800N-{size}-{type} Blackbody + Controller",
                    ListOfTestsEntry = "IR Source: SR-800N",
                    TestProcedureSection = $@"SR800N-{size} black body
Test Procedure: 
Verify that the ATP for the SR-800N-{size} is completed according to the procedure          described in doc. #TA7041000010. 
Test Radiometric offset according to procedure D1506034250.
Record the test results in the ATR document."
                };
            }
        }

        // Source Stage Section
        public static ComponentSection SourceStage()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "Motorized Source Stage",
                ListOfTestsEntry = "Motorized Source Stage",
                TestProcedureSection = @"Motorized Source Stage
Test Procedure:
Verify the Source stage travels correctly to the center of each Source from Home position.
Make sure that during this travel, there are no impacts and that the travel is smooth. Make sure no cables are tangled due to the Source Stage travel.
Repeat steps 1 & 2, for 3 times.
Record the test results in the ATR document."
            };
        }

        // Focus Stage Section
        public static ComponentSection FocusStage(string finiteDistance, string finiteDistance2, string finiteDistance3, string systemType, string variant)
        {
            // Build test procedure with multiple finite distances
            var procedureLines = new System.Text.StringBuilder();
            procedureLines.AppendLine("Motorized Focus Stage");
            procedureLines.AppendLine("Test Procedure:");

            // Add line for each non-empty finite distance
            if (!string.IsNullOrWhiteSpace(finiteDistance))
            {
                double distanceFromFocalPlane = CalculateDistanceFromFocalPlane(systemType, variant, finiteDistance);
                string distanceText = distanceFromFocalPlane.ToString("0.00");
                procedureLines.AppendLine($"Verify the focus stage travel allows movement of +{distanceText}±5% mm from Infinity position, reaching {finiteDistance}m finite focal distance.");
            }

            if (!string.IsNullOrWhiteSpace(finiteDistance2))
            {
                double distanceFromFocalPlane2 = CalculateDistanceFromFocalPlane(systemType, variant, finiteDistance2);
                string distanceText2 = distanceFromFocalPlane2.ToString("0.00");
                procedureLines.AppendLine($"Verify the focus stage travel allows movement of +{distanceText2}±5% mm from Infinity position, reaching {finiteDistance2}m finite focal distance.");
            }

            if (!string.IsNullOrWhiteSpace(finiteDistance3))
            {
                double distanceFromFocalPlane3 = CalculateDistanceFromFocalPlane(systemType, variant, finiteDistance3);
                string distanceText3 = distanceFromFocalPlane3.ToString("0.00");
                procedureLines.AppendLine($"Verify the focus stage travel allows movement of +{distanceText3}±5% mm from Infinity position, reaching {finiteDistance3}m finite focal distance.");
            }

            procedureLines.AppendLine("Make sure that during the travel, there are no impacts and that the travel is smooth. Make sure no cables are tangled due to the focus stage travel.");
            procedureLines.AppendLine("Repeat the previous two steps three times.");
            procedureLines.Append("Write Pass/Fail in the ATR document.");

            return new ComponentSection
            {
                TableOfContentsEntry = "Motorized Focus Stage",
                ListOfTestsEntry = "Motorized Focus Stage",
                TestProcedureSection = procedureLines.ToString()
            };
        }

        /// <summary>
        /// Calculates the distance from focal plane using the thin lens equation
        /// Based on Finite Distance Calc.xml
        /// Formula: Distance from focal plane = (EFL²) / (DesiredFiniteDistance - EFL)
        /// </summary>
        private static double CalculateDistanceFromFocalPlane(string systemType, string variant, string finiteDistanceMeters)
        {
            // Parse finite distance (in meters)
            if (!double.TryParse(finiteDistanceMeters, out double finiteDistanceM))
            {
                return 0;
            }

            // Get EFL in mm based on system type and variant
            double eflMm = GetEFL(systemType, variant);

            if (eflMm == 0)
            {
                return 0;
            }

            // Convert finite distance from meters to mm
            double finiteDistanceMm = finiteDistanceM * 1000;

            // Thin lens equation: Distance from focal plane = (EFL²) / (DesiredFiniteDistance - EFL)
            double distanceFromFocalPlane = (eflMm * eflMm) / (finiteDistanceMm - eflMm);

            return distanceFromFocalPlane;
        }

        /// <summary>
        /// Returns the Effective Focal Length (EFL) in mm based on system type and variant
        /// Values are from Finite Distance Calc.xml
        /// </summary>
        private static double GetEFL(string systemType, string variant)
        {
            if (string.IsNullOrEmpty(systemType))
                return 0;

            systemType = systemType.ToUpper();
            variant = variant?.ToUpper() ?? "";

            if (systemType == "METS")
            {
                return variant switch
                {
                    "VS" => 40 * (1023.3 / 1016) * 25.4,  // 40" ª correction factor ª mm per inch = 1,023.3 mm
                    "S" => 70 * (1787.1 / 1778) * 25.4,   // 70" ª correction factor ª mm per inch = 1,787.1 mm
                    "L" => 70 * (1787.1 / 1778) * 25.4,   // 70" ª correction factor ª mm per inch = 1,787.1 mm
                    "VL" => 120 * 25.4,                    // 120" ª mm per inch = 3,048 mm
                    _ => 0
                };
            }
            else if (systemType == "ILET")
            {
                return variant switch
                {
                    "4" => 30 * 25.4,   // 30" ª mm per inch = 762 mm
                    "5" => 30 * 25.4,   // 30" ª mm per inch = 762 mm (using the larger option)
                    "6" => 40 * 25.4,   // 40" ª mm per inch = 1,016 mm
                    _ => 0
                };
            }
            else if (systemType == "WFOV")
            {
                return 100;  // 100 mm
            }

            return 0;
        }

        // Target Wheel Section (Always included)
        public static ComponentSection TargetWheel()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "Motorized Target Wheel",
                ListOfTestsEntry = "Target Wheel & Targets",
                TestProcedureSection = @"Target Wheel & Targets
Test Procedure: 
Verify target wheel operation, accurate target positioning and smooth rotation.
Write Pass/Fail in the ATR document."
            };
        }

        // LOS Laser Section
        public static ComponentSection LOSLaser()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "LOS laser diode",
                ListOfTestsEntry = "LOS laser diode",
                TestProcedureSection = @"LOS laser diode
Test Procedure:
Verify that the laser diode can be operated ON/OFF VIA the controller or device center SW.
Verify that the laser beam is properly aligned towards the center of the exit aperture.
Verify that there is warning sticker on the laser.
Record the test results in the ATR document"
            };
        }

        // Backlight Section
        public static ComponentSection Backlight(string type)
        {
            // Different procedure for Fiber Optic vs LED
            if (type.Equals("Fiber Optic", StringComparison.OrdinalIgnoreCase))
            {
                return new ComponentSection
                {
                    TableOfContentsEntry = "Fiber optic Backlight",
                    ListOfTestsEntry = "Fiber optic Backlight",
                    TestProcedureSection = @"Fiber optic Backlight
Test Procedure:
Turn on the Light Source of the Backlight and activate the Backlight. Verify the Backlight illuminates.
Change the intensity using the regulator knob, verify the intensity of he Backlight is actually changing.
Write Pass/Fail in the ATR document."
                };
            }
            else
            {
                // For LED type (default)
                return new ComponentSection
                {
                    TableOfContentsEntry = "LED Backlight Source",
                    ListOfTestsEntry = "LED Backlight Source",
                    TestProcedureSection = @"LED Backlight Source
Test Procedure:
Verify that the Back Light can be operated ON/OFF VIA the controller or device center SW.
Verify that the Back Light source intensity can be controlled by the controller or device center.
Check backlight intensity range with Minolta - from 0% to 100% intensity.
Record the test results in the ATR document."
                };
            }
        }

        // Manual Choke Section
        public static ComponentSection ManualChoke()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "Manual Choke Mechanism",
                ListOfTestsEntry = "Manual Choke Mechanism",
                TestProcedureSection = @"Manual Choke Mechanism
Test Procedure:
Verify that choke mechanism handle moves smoothly through the two following positions: • Handle is completely out: the Black-Body in front of a target. • Handle maximum in, the Back-Light in front of the target.
Repeat the position change 5 times, verify the changes continue smoothly.
Write Pass/Fail in the ATR document."
            };
        }

        // QTH Lamp Section
        public static ComponentSection QTHLamp()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "QTH Lamp",
                ListOfTestsEntry = "QTH Lamp",
                TestProcedureSection = @"QTH Lamp
Test Procedure:
Perform a visual check of the QTH Lamp
Attach manufacturer COC
Perform functionality test.
Record the test results in the ATR document."
            };
        }

        // XY Stage Section
        public static ComponentSection XYStage()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "XY stage",
                ListOfTestsEntry = "XY stage",
                TestProcedureSection = @"XY stage
Test Procedure:
Move XY stage to end of each axis.
Verify that XY reaches both ends of each axis.
Verify that both power meter and laser meter can be placed in every position inside the collimator's aperture."
            };
        }

        // Power Meter Section
        public static ComponentSection PowerMeter()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "Power Meter",
                ListOfTestsEntry = "Power Meter",
                TestProcedureSection = @"Power Meter
Test Procedure:
""Shoot"" continues laser directly on power meter. Collect results.
Validate that optical path power loss is calculated from direct and through-collimator measurements and implemented in the system.
Record the result in the ATR."
            };
        }

        // Energy Meter Section
        public static ComponentSection EnergyMeter()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "Energy Meter",
                ListOfTestsEntry = "Energy Meter",
                TestProcedureSection = @"Energy Meter
Test Procedure:
""Shoot"" continues laser directly on energy meter. Collect results.
Validate that optical path energy loss is calculated from direct and through-collimator measurements and implemented in the system.
Record the result in the ATR."
            };
        }

        // CTE Section
        public static ComponentSection CTE()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "CTE",
                ListOfTestsEntry = "CTE",
                TestProcedureSection = @"CTE
Test Procedure:
Open Device Center on the PC and verify that SW works ok.
Make sure all components are connected (GREEN status color)
Verify that sources are being controlled from SW.
Record results in the ATR."
            };
        }

        // CTE Initiated Self-Test Section (second section for CTE)
        public static ComponentSection CTEInitiatedSelfTest()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "Initiated Self-Test",
                ListOfTestsEntry = "Initiated Self-Test",
                TestProcedureSection = @"Initiated Self-Test
Requirement: The initiated self-test shall test the TESTER devices
Test Method: Test
Test Procedure:
Run the initiated self-test via the TESTER – CTE software.
The following items are checked: SR800N-4D Blackbody and Target Wheel.
Verify that the test ends with Pass/Fail message for each item. Write Pass/Fail in the ATR document."
            };
        }

        // Rackmount Section
        public static ComponentSection Rackmount()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "Rackmount",
                ListOfTestsEntry = "Rackmount",
                TestProcedureSection = @"Rackmount
Test Procedure:
Compare the rackmount layout with the Rackmount design (scheme in Appendix A). Verify all the modules are in place.
Visual check – verify the Rackmount has no scratches and dents. The rackmount is clean overall.
Verify the Rackmount passed ""Electrical Safety Inspection"" test. Attach the test documentation.
Write Pass/Fail in the ATR document."
            };
        }

        // Gimbal Section
        public static ComponentSection Gimbal(string size, bool hasJoystick = false)
        {
            string joystickLines = hasJoystick ? @"
Verify proper connectivity between the joystick and the NewPort controller.
Confirm that the joystick provides full  control of the motors.
Ensure the stages can be driven to their full range of motion in all directions using the joystick." : "";

            return new ComponentSection
            {
                TableOfContentsEntry = $"{size}\" wide Gimbal",
                ListOfTestsEntry = $"{size}\" wide Gimbal",
                TestProcedureSection = $@"{size}"" wide Gimbal
Test Procedure:
Verify that the ATP for the Gimbal, is complete according to the Manufacturing Testing and Calibration form.
Record the test results in the ATR document.{joystickLines}"
            };
        }

        // LOS Alignment Target Section
        public static ComponentSection LOSAlignmentTarget()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "LOS alignment target",
                ListOfTestsEntry = "LOS alignment target",
                TestProcedureSection = @"LOS alignment target
Test Procedure:
Perform a visual check of the LOS alignment LASER.
Attach LOS camera manufacturer COC.
Perform functionality test – using a reference mirror, make sure the light spot coming back from the target LED, is detectable and can be aligned to the center of the target cross.
Turn on the Halogen lamp in the LOS Pinhole. Verify the lamp is working and the intensity is not creating saturation, when looking with the Camera.
Record the test results in the ATR document."
            };
        }

        // Device Center Section
        public static ComponentSection DeviceCenter()
        {
            return new ComponentSection
            {
                TableOfContentsEntry = "Device Center",
                ListOfTestsEntry = "Device Center",
                TestProcedureSection = @"Device Center
Test Procedure:
Open Device Center on the PC and verify the SW is configured to the HW.
Verify all the features are controlled from SW.
Make sure all components are connected (GREEN status color)
Record results in the ATR."
            };
        }

        // VRS Section
        public static ComponentSection VRS(string vrs1, string vrs2, string vrs3)
        {
            // Build the wavelengths sticker verification line
            string wavelengthsLine = "Verify that the wavelengths stickers are: ";

            List<string> wavelengths = new List<string>();
            if (!string.IsNullOrEmpty(vrs1) && !vrs1.Equals("NA", StringComparison.OrdinalIgnoreCase))
                wavelengths.Add(vrs1);
            if (!string.IsNullOrEmpty(vrs2) && !vrs2.Equals("NA", StringComparison.OrdinalIgnoreCase))
                wavelengths.Add(vrs2);
            if (!string.IsNullOrEmpty(vrs3) && !vrs3.Equals("NA", StringComparison.OrdinalIgnoreCase))
                wavelengths.Add(vrs3);

            wavelengthsLine += string.Join(",", wavelengths);

            return new ComponentSection
            {
                TableOfContentsEntry = "VRS",
                ListOfTestsEntry = "VRS",
                TestProcedureSection = $@"VRS
Test Procedure:
Turn VRS ""ON"" using the switch and in the CTE.
Verify that oscillator on the fiber works.
Set the range to measure, minimum at 300[m] and maximum at 25000[m].
""Shoot"" laser at various ranges: 300, 500, 1000, 5000 & 10000.
Verify that range result is similar as the range above and that one target is identified.
{wavelengthsLine}
Record the result in the ATR."
            };
        }

        // NewPort Stage Section
        public static ComponentSection NewPortStage(string maxWeight, bool hasJoystick = false)
        {
            string joystickLines = hasJoystick ? @"
Verify proper connectivity between the joystick and the NewPort controller.
Confirm that the joystick provides full  control of the motors.
Ensure the stages can be driven to their full range of motion in all directions using the joystick." : "";

            return new ComponentSection
            {
                TableOfContentsEntry = $"METS UUT<{maxWeight}Kg Stage",
                ListOfTestsEntry = $"METS UUT<{maxWeight}Kg Stage",
                TestProcedureSection = $@"METS UUT<{maxWeight}Kg Stage
Test Procedure:
Perform a visual check of the UUT Stage & Controller.
Attach manufacturer COC.
Perform functionality test.
Record the test results in the ATR document.{joystickLines}"
            };
        }
    }

    public class ComponentSection
    {
        public string TableOfContentsEntry { get; set; } = "";
        public string ListOfTestsEntry { get; set; } = "";
        public string TestProcedureSection { get; set; } = "";
    }
}
