using System.ComponentModel.DataAnnotations;

namespace MillPlane
{
    public class BuildSettings
    {
        [Required]
        [Range(0.0001, Double.MaxValue, ErrorMessage = "Tool diameter must be greater than 0")]
        public double ToolDiameter { get; set; } = 1.0;

        [Required]
        [Range(0.0001, Double.MaxValue, ErrorMessage = "Depth must be greater than 0")]
        public double Depth { get; set; } = 0.0625;

        [Range(1, Int32.MaxValue, ErrorMessage = "RPM must be greater than 0")]
        public int Rpm { get; set; } = 12000;

        [Range(0.0001, Double.MaxValue, ErrorMessage = "Feed rate must be greater than 0")]
        public double Feed { get; set; } = 30.0;

        public double StepOver { get; set; }

        public double StepDown { get; set; }

        public double MaterialWidth { get; set; }

        public double MaterialHeight { get; set; }

        public double ToolRadius => ToolDiameter / 2;
        public double XStart => (0 - ToolRadius);
        public double XEnd => (Width + ToolRadius);
        public double Height => (MaterialHeight < ToolDiameter) ? ToolDiameter : MaterialHeight;
        public double Width => (MaterialWidth < ToolDiameter) ? ToolDiameter : MaterialWidth;

        public BuildSettings()
        {

        }
    }
}
