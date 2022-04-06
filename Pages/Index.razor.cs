using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace MillPlane.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject]
        protected ILogger<Index> Logger { get; set; } = default!;

        [Inject]
        protected IJSRuntime JS { get; set; } = default!;

        private BuildSettings _model = new BuildSettings();
        private EditContext? _editContext;

        protected override async Task OnInitializedAsync()
        {
            _editContext = new EditContext(_model);

            await Task.CompletedTask;
        }

        private async Task HandleValidSubmit()
        {
            if (_editContext != null && _editContext.Validate())
            {
                var gcode = Build((BuildSettings)_editContext.Model);
                using var stream = new MemoryStream(Encoding.ASCII.GetBytes(gcode));
                using var streamRef = new DotNetStreamReference(stream: stream);

                await JS.InvokeVoidAsync("downloadFileFromStream", "millplane.gcode", streamRef);
            }
            else
            {
                Logger.LogWarning("Form is INVALID");
            }
        }

        private static string Build(BuildSettings buildSettings)
        {
            double toolDiameter = buildSettings.ToolDiameter;
            int rpm = buildSettings.Rpm;
            double feed = buildSettings.Feed;
            double stepOver = buildSettings.StepOver;
            double stepDown = buildSettings.StepDown;
            double width = buildSettings.Width;
            double height = buildSettings.Height;
            double depth = buildSettings.Depth;

            if (depth > 0) depth = 0 - depth;
            if (stepOver <= 0) stepOver = toolDiameter * 0.4;
            if (stepDown <= 0) stepDown = toolDiameter * 0.1;

            var toolRadius = (toolDiameter / 2);
            var startX = buildSettings.XStart;
            var endX = buildSettings.XEnd;

            // var stream = new MemoryStream();
            // var writer = new StreamWriter(stream);

            using var writer = new StringWriter();

            writer.NewLine = "\n";

            writer.WriteLine("%");
            // Set units to inches
            writer.WriteLine("G20");
            // Switch to absolute distance mode
            writer.WriteLine("G90");
            // Move to origin
            writer.WriteLine("G0X0.000Y0.000Z0.125");

            // Spindle off
            writer.WriteLine("M5");
            // Tool comment
            writer.WriteLine($"(TOOL/MILL,{toolDiameter:0.000}, 0.000, 0.000, 0.00)");
            // Change tool to tool 1
            writer.WriteLine("M6T1");
            // Spindle on
            writer.WriteLine($"M03S{rpm}");
            // Move tool to initial cut origin)
            writer.WriteLine($"G0X{Math.Round(startX, 4)}Y0.000");
            writer.WriteLine("G0Z0.125");

            var currentHeight = 0.0;
            var currentDepth = 0.0;

            while (currentDepth > depth)
            {
                // Step down
                currentDepth -= stepDown;

                // Ensure step down does not exceed final depth
                if (currentDepth < depth)
                {
                    currentDepth = depth;
                }

                while (currentHeight <= height)
                {
                    // Plunge Z to starting cut depth
                    writer.WriteLine($"G1Z{Math.Round(currentDepth, 4)}F10.0");

                    // Cutting pass
                    writer.WriteLine($"G1X{Math.Round(endX, 4)}F{feed:0.0##}");

                    // Retract and return to start of pass
                    writer.WriteLine($"G0X{Math.Round(startX, 4)}Y{Math.Round(currentHeight, 4)}Z0.125");

                    // Increment height
                    currentHeight += stepOver;

                    // Rapid to start of pass
                    writer.WriteLine($"G0X{Math.Round(startX, 4)}Y{Math.Round(currentHeight, 4)}Z0.125");
                }
            }

            // Retract Z
            writer.WriteLine("G0Z0.250");

            // Spindle off
            writer.WriteLine("M5");

            // End program
            writer.WriteLine("M30");
            writer.WriteLine("(END)");
            writer.Flush();

            var gcode = writer.ToString();

            writer.Close();

            return gcode;
            // stream.Seek(0, SeekOrigin.Begin);
            // return stream;
        }
    }
}
