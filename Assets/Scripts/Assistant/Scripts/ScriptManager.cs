using System;
using System.Collections.Generic;
using UOScript;
using Assistant.Scripts;

using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps;

namespace Assistant
{
    internal static class ScriptManager
    {
        private static bool _Recording;
        internal static bool Recording 
        {
            get => _Recording;
            set
            {
                if(value != _Recording)
                {
                    _Recording = value;
                    if(_Recording)
                    {
                        _Timer?.Stop();
                        _Timer = new ActionsTimer();
                        _Timer.Start();
                    }
                    else
                    {
                        _Timer?.Stop();
                    }
                }
            }
        }

        internal static bool Running => ScriptRunning;

        private static bool ScriptRunning { get; set; }

        internal static DateTime LastWalk { get; set; }

        internal static bool SetLastTargetActive { get; set; }

        internal static bool SetVariableActive { get; set; }

        private class ScriptTimer : Timer
        {
            // Only run scripts once every 25ms to avoid spamming.
            internal ScriptTimer() : base(TimeSpan.FromMilliseconds(5), TimeSpan.FromMilliseconds(5))
            {
            }

            protected override void OnTick()
            {
                if (Interpreter.ExecuteScript())
                {
                    if (ScriptRunning == false)
                    {
                        Begin();
                    }
                }
                else
                {
                    if (ScriptRunning)
                    {
                        if (!Interpreter.HasQueuedScripts())
                        {
                            End();
                        }
                        else
                        {
                            Script s = Interpreter.GetFirstValidQueued();
                            if(s != null)
                            {
                                Interpreter.StartScript(s);
                            }
                            else
                            {
                                End();
                            }
                        }
                    }
                }
            }

            private void Begin()
            {
                UOSObjects.Player?.SendMessage("Running Script");
                ScriptRunning = true;
                if (UOSObjects.Gump != null)
                {
                    UOSObjects.Gump.PlayMacro.TextLabel.Text = "Stop";
                    UOSObjects.Gump.RecordMacro.TextLabel.Hue = ScriptTextBox.RED_HUE;
                    UOSObjects.Gump.RecordMacro.IsEnabled = false;
                }
            }

            private void End()
            {
                UOSObjects.Player?.SendMessage("Script Ended");
                ScriptRunning = false;
                if (UOSObjects.Gump != null)
                {
                    UOSObjects.Gump.PlayMacro.TextLabel.Text = "Play";
                    UOSObjects.Gump.RecordMacro.TextLabel.Hue = ScriptTextBox.GRAY_HUE;
                    UOSObjects.Gump.RecordMacro.IsEnabled = true;
                }
            }
        }

        internal static void StopScript()
        {
            Interpreter.StopScript();
        }

        internal static bool PlayScript(string scriptName, bool fromscript = false)
        {
            if (string.IsNullOrEmpty(scriptName))
                return false;
            MacroDictionary.TryGetValue(scriptName, out HotKeyOpts opts);
            if (!fromscript)
            {
                string old = ScriptRunning ? Interpreter.ActiveScript?.ScriptName : null;
                if (!string.IsNullOrEmpty(old) && MacroDictionary.TryGetValue(old, out HotKeyOpts oldopts) && oldopts.NoAutoInterrupt)
                    return false;
            }
            PlayScript(opts, fromscript);
            return true;
        }

        internal static void PlayScript(HotKeyOpts macro, bool fromscript = false, bool frombutton = false)
        {
            if (UOSObjects.Player == null || macro == null || string.IsNullOrEmpty(macro.Macro))//ScriptEditor == null || 
                return;

            if(fromscript && UOSObjects.Gump.ReturnToParentScript && Engine.Instance.AllowBit(FeatureBit.LoopingMacros))
                Interpreter.Enqueue(Interpreter.ActiveScript);
            if(ScriptRunning && frombutton)
            {
                StopScript();
                UOSObjects.Gump.PlayMacro.TextLabel.Text = "Play";
                UOSObjects.Gump.RecordMacro.IsEnabled = true;
                return;
            }
            StopScript(); // be sure nothing is running
            
            SetLastTargetActive = false;
            SetVariableActive = false;
            ASTNode node;
            if (Engine.Instance.AllowBit(FeatureBit.LoopingMacros) && macro.Loop && !fromscript)
                node = Lexer.Lex($"{macro.Macro}\nloop", frombutton);
            else
                node = Lexer.Lex(macro.Macro, frombutton);
            Interpreter.StartScript(new Script(node, macro.Param));
        }

        private static ScriptTimer Timer { get; } = new ScriptTimer();
        private static bool _Initialized = false;

        internal static void OnLogin()
        {
            if (!_Initialized)
            {
                _Initialized = true;
                Commands.Register();
                //AgentCommands.Register();
                Aliases.Register();
                Expressions.Register();
            }
            Timer.Start();
        }

        internal static void OnLogout()
        {
            Timer.Stop();
        }

        internal static SortedDictionary<string, HotKeyOpts> MacroDictionary { get; } = new SortedDictionary<string, HotKeyOpts>();
        internal static Queue<Action> Actions = new Queue<Action>();
        private static ActionsTimer _Timer;

        internal class ActionsTimer : Timer
        {
            internal ActionsTimer() : base(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            {

            }

            protected override void OnTick()
            {
                if (Actions.Count > 0)
                    Actions.Dequeue().Invoke();
            }
        }

        internal static bool QueueToScript(Action action)
        {
            if(Recording)
            {
                Actions.Enqueue(action);
                return true;
            }
            return false;
        }

        internal static void AddToScript(string command)
        {
            var box = UOSObjects.Gump.MacroBox;
            if (box != null)
            {
                if(box.CaretIndex > 0 && box.Text.Length > box.CaretIndex && box.Text[box.CaretIndex - 1] != '\n')
                    UOSObjects.Gump.MacroBox.AppendText($"\n{command}\n");
                else
                    UOSObjects.Gump.MacroBox.AppendText($"{command}\n");
            }
        }

        internal static void Error(bool quiet, string message, string scripterror = null)
        {
            if (!quiet)
            {
                if (scripterror != null)
                    UOSObjects.Player?.SendMessage(MsgLevel.Error, $"'{scripterror}' - {message}");
                else
                    UOSObjects.Player?.SendMessage(MsgLevel.Error, $"{message}");
            }
        }

        internal class ToolTipDescriptions
        {
            internal string Title;
            internal string[] Parameters;
            internal string Returns;
            internal string Description;
            internal string Example;

            internal ToolTipDescriptions(string title, string[] parameter, string returns, string description,
                string example)
            {
                Title = title;
                Parameters = parameter;
                Returns = returns;
                Description = description;
                Example = example;
            }

            internal string ToolTipDescription()
            {
                string complete_description = string.Empty;

                complete_description += "Parameter(s): ";

                foreach (string parameter in Parameters)
                    complete_description += "\n\t" + parameter;

                complete_description += "\nReturns: " + Returns;

                complete_description += "\nDescription:";

                complete_description += "\n\t" + Description;

                complete_description += "\nExample(s):";

                complete_description += "\n\t" + Example;

                return complete_description;
            }
        }

        internal static Dictionary<string, int> SkillMap = new Dictionary<string, int>()
        {
            { "alchemy", 0 },
            { "anatomy", 1 },
            { "animallore", 2 }, { "animal lore", 2 },
            { "itemidentification", 3 }, {"itemid", 3 }, { "item identification", 3 }, { "item id", 3 },
            { "armslore", 4 }, { "arms lore", 4 },
            { "parry", 5 }, { "parrying", 5 },
            { "begging", 6 },
            { "blacksmith", 7 }, { "blacksmithing", 7 },
            { "fletching", 8 }, { "bowcraft", 8 },
            { "peacemaking", 9 }, { "peace", 9 }, { "peacemake", 9 },
            { "camping", 10 }, { "camp", 10 },
            { "carpentry", 11 },
            { "cartography", 12 },
            { "cooking", 13 }, { "cook", 13 },
            { "detectinghidden", 14 }, { "detect", 14 }, { "detecthidden", 14 }, { "detecting hidden", 14 }, { "detect hidden", 14 },
            { "discordance", 15 }, { "discord", 15 }, { "enticement", 15 }, { "entice", 15 },
            { "evaluatingintelligence", 16 }, { "evalint", 16 }, { "eval", 16 }, { "evaluating intelligence", 16 },
            { "healing", 17 },
            { "fishing", 18 },
            { "forensicevaluation", 19 }, { "forensiceval", 19 }, { "forensics", 19 },
            { "herding", 20 },
            { "hiding", 21 },
            { "provocation", 22 }, { "provo", 22 },
            { "inscription", 23 }, { "scribe", 23 },
            { "lockpicking", 24 },
            { "magery", 25 }, { "mage", 25 },
            { "magicresist", 26 }, { "resist", 26 }, { "resistingspells", 26 },
            { "tactics", 27 },
            { "snooping", 28 }, { "snoop", 28 },
            { "musicianship", 29 }, { "music", 29 },
            { "poisoning", 30 },
            { "archery", 31 },
            { "spiritspeak", 32 },
            { "stealing", 33 },
            { "tailoring", 34 },
            { "taming", 35 }, { "animaltaming", 35 }, { "animal taming", 35 },
            { "tasteidentification", 36 }, { "tasteid", 36 },
            { "tinkering", 37 },
            { "tracking", 38 },
            { "veterinary", 39 }, { "vet", 39 },
            { "swords", 40 }, { "swordsmanship", 40 },
            { "macing", 41 }, { "macefighting", 41 }, { "mace fighting", 41 },
            { "fencing", 42 },
            { "wrestling", 43 },
            { "lumberjacking", 44 },
            { "mining", 45 },
            { "meditation", 46 },
            { "stealth", 47 },
            { "removetrap", 48 },
            { "necromancy", 49 }, { "necro", 49 },
            { "focus", 50 },
            { "chivalry", 51 },
            { "bushido", 52 },
            { "ninjitsu", 53 },
            { "herboristery", 54 }
        };

        // Convert steam-compatible skill names to Skills
        internal static Skill GetSkill(string skillName)
        {
            if (SkillMap.TryGetValue(skillName.ToLower(), out var id))
                return UOSObjects.Player.Skills[id];

            new RunTimeError(null, $"Unknown skill name: {skillName}");
            return null; 
        }
    }
}
