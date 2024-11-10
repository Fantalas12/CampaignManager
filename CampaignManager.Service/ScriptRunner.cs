/* using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Text.Json;

namespace CampaignManager.Persistence.Models
{
    public class ScriptRunner
    {
        public async Task<string> RunScriptAsync(Generator generator, Note note)
        {
            if (generator.Script == null)
            {
                throw new ArgumentNullException(nameof(generator.Script), "Script cannot be null.");
            }

            var options = ScriptOptions.Default
                .AddReferences(AppDomain.CurrentDomain.GetAssemblies())
                .AddImports("System", "System.Linq", "System.Collections.Generic", "System.Text.Json");
            

            var options = ScriptOptions.Default
            .AddReferences(
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(JsonSerializer).Assembly,
                typeof(Random).Assembly, // For random number generation
                typeof(List<>).Assembly, // For list handling
                typeof(Array).Assembly, // For array handling
                typeof(String).Assembly // For text manipulations
            )
            .AddImports(
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "System.Text.Json",
                "System.Text" // For text manipulations
            );

            var script = generator.Script;

            var globals = new ScriptGlobals { NoteContent = note.Content };

            try
            {
                var result = await CSharpScript.EvaluateAsync<string>(script, options, globals);
                return result;
            }
            catch (CompilationErrorException ex)
            {
                // Log compilation errors
                Console.WriteLine("Script compilation errors: " + string.Join(Environment.NewLine, ex.Diagnostics));
                throw;
            }
            catch (Exception ex)
            {
                // Log runtime errors
                Console.WriteLine("Script execution error: " + ex.Message);
                throw;
            }
        }
    }

    public class ScriptGlobals
    {
        public string? NoteContent { get; set; }
    }
} */
