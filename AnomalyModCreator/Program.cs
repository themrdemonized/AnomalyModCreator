using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnomalyModCreator
{
    internal class Program
    {

        // Overwrite Choices
        enum overwriteChoices
        {
            overwriteYes,
            overwriteNo,
            overwriteYesAll,
            overwriteNoAll
        };
        static Dictionary<string, overwriteChoices> overwriteStrings = new Dictionary<string, overwriteChoices>()
        {
            {"y", overwriteChoices.overwriteYes},
            {"Y", overwriteChoices.overwriteYes},
            {"n", overwriteChoices.overwriteNo},
            {"N", overwriteChoices.overwriteNo},
            {"a", overwriteChoices.overwriteYesAll},
            {"A", overwriteChoices.overwriteYesAll},
            {"i", overwriteChoices.overwriteNoAll},
            {"I", overwriteChoices.overwriteNoAll},
        };

        // Sanitize input
        // Remove extra spaces and invalid chars
        static string sanitizeString(string s)
        {
            s = s.Trim();
            s = Regex.Replace(s, "  +", "");
            foreach (var c in Path.GetInvalidPathChars())
            {
                s = s.Replace(c.ToString(), String.Empty);
            }
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c.ToString(), String.Empty);
            }
            return s;
        }

        static void Main(string[] args)
        {
            var modName = args.ElementAtOrDefault(0);
            
            if (modName == null)
            {
                Console.WriteLine("Enter the name of new mod: ");
                var n = Console.ReadLine();
                if (n == null || n.Length == 0)
                {
                    Console.WriteLine("You have to specify Mod Name");
                    Console.ReadLine();
                    return;
                }

                modName = sanitizeString(n);
            } else
            {
                // If mod name is provided as string with no quotes (My New Mod), then concat arguments
                for (var i = 1; args.ElementAtOrDefault(i) != null; i++)
                {
                    modName += $" {args.ElementAtOrDefault(i)}";
                }
                modName = sanitizeString(modName);
            }
                       

            var underscoreModName = modName.Replace(" ", "_").ToLower();
            Console.WriteLine($"Creating Mod {modName}");

            // Directories
            var paths = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>()
                {
                        {"type", "directory"},
                        {"name", $".\\{modName}\\gamedata\\configs\\text\\eng"}
                },
                new Dictionary<string, string>()
                {
                        {"type", "directory"},
                        {"name", $".\\{modName}\\gamedata\\configs\\text\\rus"}
                },
                new Dictionary<string, string>()
                {
                        {"type", "directory"},
                        {"name", $".\\{modName}\\gamedata\\scripts"}
                },

                // Files
                new Dictionary<string, string>()
                {
                        {"type", "file"},
                        {"name", $".\\{modName}\\gamedata\\scripts\\{underscoreModName}.script"},
                        {"content", $@"
-- {modName}
-- Generated with AnomalyModCreator

-- MCM
function load_defaults()
	local t = {{}}
	local op = {underscoreModName}_mcm.op
	for i, v in ipairs(op.gr) do
		if v.def ~= nil then
			t[v.id] = v.def
		end
	end
	return t
end

settings = load_defaults()

function load_settings()
	settings = load_defaults()
	if ui_mcm then
		for k, v in pairs(settings) do
			settings[k] = ui_mcm.get(""{underscoreModName}/"" .. k)
		end
	end
	return settings
end

function on_game_start()
	RegisterScriptCallback(""actor_on_first_update"", load_settings)
	RegisterScriptCallback(""on_option_change"", load_settings)
end
                        "}
                },

                new Dictionary<string, string>()
                {
                        {"type", "file"},
                        {"name", $".\\{modName}\\gamedata\\scripts\\{underscoreModName}_mcm.script"},
                        {"content", $@"
-- {modName}
-- Generated with AnomalyModCreator

op = {{ id= ""{underscoreModName}"",sh=true ,gr={{
		{{ id= ""title"", type= ""slide"", link= ""ui_options_slider_player"", text=""ui_mcm_{underscoreModName}_title"", size= {{512,50}}, spacing= 20 }},
	}}
}}

function on_mcm_load()
	return op
end
                        "}
                },

                new Dictionary<string, string>()
                {
                        {"type", "file"},
                        {"name", $".\\{modName}\\gamedata\\configs\\text\\eng\\ui_st_{underscoreModName}.xml"},
                        {"content", $@"
<!--
    {modName}
    Generated with AnomalyModCreator
-->

<?xml version=""1.0"" encoding=""windows-1251""?>

<!-- 
	MCM
-->
<string_table>

	<string id=""ui_mcm_{underscoreModName}_title"">
		<text>{modName}</text>
	</string>
	<string id=""ui_mcm_menu_{underscoreModName}"">
		<text>{modName}</text>
	</string>

</string_table>
                        "}
                },

                new Dictionary<string, string>()
                {
                        {"type", "file"},
                        {"name", String.Format(".\\{0}\\gamedata\\configs\\text\\rus\\ui_st_{1}.xml", modName, underscoreModName)},
                        {"content", $@"
<!--
    {modName}
    Generated with AnomalyModCreator
-->

<?xml version=""1.0"" encoding=""windows-1251""?>

<!-- 
	MCM
-->
<string_table>

	<string id=""ui_mcm_{underscoreModName}_title"">
		<text>{modName}</text>
	</string>
	<string id=""ui_mcm_menu_{underscoreModName}"">
		<text>{modName}</text>
	</string>

</string_table>
                        "}
                }
            };

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string overwriteChoice = null;

            foreach (var path in paths)
            {
                void createFile()
                {
                    Console.WriteLine($"Creating file {path["name"]}");
                    Console.WriteLine("");
                    System.IO.File.WriteAllText(path["name"], path["content"], Encoding.GetEncoding(1251));
                }

                if (path["type"] == "directory")
                {
                    Console.WriteLine($"Creating directory {path["name"]}");
                    Console.WriteLine("");
                    Directory.CreateDirectory(path["name"]);
                } else if (path["type"] == "file")
                {
                    if (File.Exists(path["name"]))
                    {
                        if (overwriteChoice != null && overwriteStrings[overwriteChoice] == overwriteChoices.overwriteYesAll)
                        {
                            createFile();
                        } else if (overwriteChoice != null && overwriteStrings[overwriteChoice] == overwriteChoices.overwriteNoAll)
                        {
                            continue;
                        } else
                        {
                            while (true)
                            {
                                Console.WriteLine($"File {path["name"]} already exists in mod folder, overwrite?");
                                Console.WriteLine("[y] - Yes");
                                Console.WriteLine("[n] - No");
                                Console.WriteLine("[a] - Yes to all occurences");
                                Console.WriteLine("[i] - No to all occurences");

                                var input = Console.ReadLine();
                                if (input == null || !overwriteStrings.ContainsKey(input))
                                {
                                    continue;
                                }
                                else
                                {
                                    overwriteChoice = input;
                                    if (overwriteStrings[overwriteChoice] == overwriteChoices.overwriteYes || overwriteStrings[overwriteChoice] == overwriteChoices.overwriteYesAll)
                                    {
                                        createFile();
                                    }
                                    break;
                                }
                            }
                        }                        
                    } else
                    {
                        createFile();
                    }                   
                }
            }

            Console.WriteLine($"Mod {modName} Created!");
            Console.ReadLine();
            return;
        }
    }
}