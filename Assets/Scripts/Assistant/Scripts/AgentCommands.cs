using System;
using UOScript;

namespace Assistant.Scripts
{
    public static class AgentCommands
    {
        public static void Register()
        {
            // "useonce", "organizer", "org", "restock", "scav", "scavenger"
            /*Interpreter.RegisterCommandHandler("useonce", UseOnceCommand, null);
            Interpreter.RegisterCommandHandler("organizer", OrganizerAgentCommand, null);
            Interpreter.RegisterCommandHandler("organize", OrganizerAgentCommand, null);
            Interpreter.RegisterCommandHandler("org", OrganizerAgentCommand, null);
            Interpreter.RegisterCommandHandler("restock", RestockAgentCommand, null);
            Interpreter.RegisterCommandHandler("scav", ScavAgentCommand, null);
            Interpreter.RegisterCommandHandler("scavenger", ScavAgentCommand, null);
            Interpreter.RegisterCommandHandler("sell", SellAgentCommand, null);*/
        }

        private static bool RestockAgentCommand(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, "Usage: restock (number) ['set']");
                return true;
            }

            int agentNum = args[0].AsInt();

            bool setBag = false;

            if (args.Length == 2)
            {
                if (args[1].AsString().IndexOf("set", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    setBag = true;
                }
            }

            if (setBag)
            {
                //TODO: Restock - Agents
                //RestockAgent.Agents[agentNum - 1].SetHB();
            }
            else
            {
                //TODO: Restock - Agents
                //RestockAgent.Agents[agentNum - 1].OnHotKey();
            }

            return true;
        }

        private static bool UseOnceCommand(string command, Argument[] args, bool quiet, bool force)
        {
            bool add = false;
            bool container = false;

            if (args.Length == 1)
            {
                if (args[0].AsString().IndexOf("add", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    add = true;
                } 
                else if (args[0].AsString().IndexOf("addcontainer", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    container = true;
                }
            }

            if (add)
            {
                //TODO: Restock - Agents
                //UseOnceAgent.Instance.OnAdd();
            }
            else if (container)
            {
                //TODO: Restock - Agents
                //UseOnceAgent.Instance.OnAddContainer();
            }
            else
            {
                //TODO: Restock - Agents
                //UseOnceAgent.Instance.OnHotKey();
            }

            return true;
        }

        private static bool OrganizerAgentCommand(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, "Usage: organizer (number) ['set']");
                return true;
            }

            int agentNum = args[0].AsInt();

            bool setBag = false;

            if (args.Length == 2)
            {
                if (args[1].AsString().IndexOf("set", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    setBag = true;
                }
            }

            if (setBag)
            {
                //TODO: Restock - Agents - Organizer
                //OrganizerAgent.Agents[agentNum - 1].SetHotBag();
            }
            else
            {
                //TODO: Restock - Agents - Organizer
                //OrganizerAgent.Agents[agentNum - 1].Organize();
            }

            return true;
        }

        private static bool ScavAgentCommand(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, "Usage: scavenger ['clear'/'add'/'on'/'off'/'set']");
                return true;
            }

            bool clear = false;
            bool add = false;
            bool set = false;

            bool status = false;
            bool enabled = true;

            if (args.Length == 1)
            {
                if (args[0].AsString().IndexOf("clear", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    clear = true;
                }
                else if (args[0].AsString().IndexOf("add", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    add = true;
                }
                else if (args[0].AsString().IndexOf("on", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    status = true;
                }
                else if (args[0].AsString().IndexOf("off", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    status = true;
                    enabled = false;
                }
                else if (args[0].AsString().IndexOf("set", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    set = true;
                }
            }

            if (clear)
            {
                Scavenger.ClearCache(true);
            }
            else if (add)
            {
                Scavenger.AddToHotBag();
            }
            else if (status)
            {
                UOSObjects.Gump.EnabledScavenger.IsChecked = enabled;
            }
            else if (set)
            {
                Scavenger.SetHotBag();
            }

            return true;
        }

        private static bool SellAgentCommand(string command, Argument[] args, bool quiet, bool force)
        {
            //TODO: Restock - Agents - Organizer - Scavenger - SELL
            //SellAgent.Instance.SetHotBag();

            return true;
        }
    }
}
