using System;

namespace System_ATP_creator
{
    public class ATPConfiguration
    {
        public string SystemSN { get; set; } = "";
        public string Contract { get; set; } = "";
        public string PMName { get; set; } = "";
        public string OrderNumber { get; set; } = "";
        public string SystemType { get; set; } = "";
        public string METSVariant { get; set; } = "";
        public string FOV { get; set; } = "";
        public string EFL { get; set; } = ""; // ILET Variant 5 only: 15" or 30" (Effective Focal Length)
        public bool HasSourceStage { get; set; }
        public bool HasRackmount { get; set; }
        public bool HasGimbal { get; set; }
        public bool HasGimbalJoystick { get; set; }
        public string GimbalSize { get; set; } = "";
        public bool HasLOSAlignmentTarget { get; set; }
        public bool HasTargetWheel { get; set; }
        public bool HasCTE { get; set; }
        public bool HasDeviceCenter { get; set; }
        public bool HasNewPortStage { get; set; }
        public bool HasNewPortStageJoystick { get; set; }
        public string NewPortStageMaxWeight { get; set; } = "";
        public bool HasFocusStage { get; set; }
        public bool HasVRS { get; set; }
        public string TargetWheelPDFPath { get; set; } = "";
        public string FocusStageFiniteDistance { get; set; } = "";
        public string FocusStageFiniteDistance2 { get; set; } = "";
        public string FocusStageFiniteDistance3 { get; set; } = "";
        public string VRS1 { get; set; } = "";
        public string VRS2 { get; set; } = "";
        public string VRS3 { get; set; } = "";
        
        // Radiation Source properties
        public bool HasBB { get; set; }
        public bool HasIS { get; set; }
        public bool HasLOSLaser { get; set; }
        public bool HasBacklight { get; set; }
        public string BacklightType { get; set; } = ""; // LED or Fiber Optic
        public bool HasQTHLamp { get; set; }
        public bool HasXYStage { get; set; }
        public bool HasPowerMeter { get; set; }
        public bool HasEnergyMeter { get; set; }
        public bool HasManualChoke { get; set; }
        public string BBType { get; set; } = ""; // RR / STD / ET / LT / WTR / HE / HT / HT-HA / CH-STD / CH-ET / CH-LT / CH-WTR / SR200N-33
        public string BBSize { get; set; } = ""; // 1D/2D/3D/4D/5D/6D/8D/10D/12D/14D/16D/20D/35D/40D
        public string ISExitAperture { get; set; } = ""; // 2"/3"/4"/5"
        public string LOSLaserWavelength { get; set; } = ""; // User input in nm
        public string SavePath { get; set; } = ""; // Custom save location for generated ATP

        /// <summary>
        /// Gets the variant distance value based on system type and variant.
        /// For METS: VS=1023.3, S=1787.1, L=1787.1, VL=3048
        /// For ILET: 4=762, 5=EFL based (15"=381, 30"=762), 6=1016
        /// </summary>
        public string GetVariantDistance()
        {
            if (SystemType.Equals("METS", StringComparison.OrdinalIgnoreCase))
            {
                return METSVariant.ToUpper() switch
                {
                    "VS" => "1023.3",
                    "S" => "1787.1",
                    "L" => "1787.1",
                    "VL" => "3048",
                    _ => ""
                };
            }
            else if (SystemType.Equals("ILET", StringComparison.OrdinalIgnoreCase))
            {
                // For ILET, the variant determines the distance.
                // Variant 5 lets the user choose the EFL (15" or 30"); insert the value in mm.
                if (METSVariant == "5" && !string.IsNullOrEmpty(EFL))
                {
                    return EFL.Replace("\"", "").Trim() switch
                    {
                        "15" => "381",   // 15" x 25.4 mm/inch = 381 mm
                        "30" => "762",   // 30" x 25.4 mm/inch = 762 mm
                        _ => "762"
                    };
                }

                // For ILET, the variant determines the distance
                return METSVariant switch
                {
                    "4" => "762",
                    "5" => "762",
                    "6" => "1016",
                    _ => ""
                };
            }
            
            return "";
        }

        public override string ToString()
        {
            return $"System: {SystemType}" +
                   (string.IsNullOrEmpty(METSVariant) ? "" : $" - {METSVariant}") +
                   $", FOV: {FOV}" +
                   $"\nComponents: " +
                   (HasSourceStage ? "Source Stage, " : "") +
                   (HasTargetWheel ? "Target Wheel, " : "") +
                   (HasFocusStage ? $"Focus Stage (Finite Distance: {FocusStageFiniteDistance})" : "") +
                   $"\nRadiation Sources: " +
                   (HasBB ? $"B.B ({BBType} - {BBSize}), " : "") +
                   (HasIS ? $"I.S (Exit Aperture: {ISExitAperture}), " : "") +
                   (HasLOSLaser ? $"LOS Laser ({LOSLaserWavelength} nm)" : "");
        }
    }
}
