using System;
using System.Collections.Generic;
using System.Linq;
using Makarov.Framework.Components;

namespace Makarov.FlowchartBuilder.Commands
{

    public abstract class Command
    {

        public class CommandException : FlowchartBuilderException
        {

            public CommandException(string message)
                : base(message ?? string.Empty)
            { }
        }

        public class CommandNotExistsException : CommandException
        {

            public CommandNotExistsException(string commandTypeName)
                : base(string.Format("Commad type '{0}' not exists.", commandTypeName ?? string.Empty))
            { }
        }

        private static readonly Dictionary<string, KeyValuePair<int, Command>> Commands1 =
            new Dictionary<string, KeyValuePair<int, Command>>();
        private static readonly Dictionary<string, KeyValuePair<int, Command>> Commands2 =
            new Dictionary<string, KeyValuePair<int, Command>>();
        private static readonly Dictionary<string, KeyValuePair<int, Command>> Commands3 =
            new Dictionary<string, KeyValuePair<int, Command>>();

        public static void FlushCache()
        {
            Commands1.Clear();
            Commands2.Clear();
            Commands3.Clear();
        }

        private const int IntercacheCounter = 20;

        public static Command GetInstance(string command)
        {
            return GetInstance(command, null);
        }

        public static Command GetInstance(string command, IActiveControl control)
        {

            if (Commands3.ContainsKey(command))
            {

                Commands3[command].Value.AddControl(control);

                return Commands3[command].Value;
            }

            if (Commands2.ContainsKey(command))
            {

                int i = Commands2[command].Key + 1;

                Command cmd = Commands2[command].Value;

                cmd.AddControl(control);

                if (i >= IntercacheCounter)
                {

                    Commands3.Add(command, new KeyValuePair<int, Command>(0, cmd));
                    Commands2.Remove(command);
                }
                else
                {

                    Commands2[command] = new KeyValuePair<int, Command>(i, cmd);
                }

                return cmd;
            }

            if (Commands1.ContainsKey(command))
            {

                int i = Commands1[command].Key + 1;

                Command cmd = Commands1[command].Value;

                cmd.AddControl(control);

                if (i >= IntercacheCounter)
                {

                    Commands2.Add(command, new KeyValuePair<int, Command>(0, cmd));
                    Commands1.Remove(command);
                }
                else
                {

                    Commands1[command] = new KeyValuePair<int, Command>(i, cmd);
                }

                return cmd;
            }

            string cmdTypeName = @"Makarov.FlowchartBuilder.Commands." + command;

            Type cmdType = Type.GetType(cmdTypeName);

            if (cmdType == null)
                throw new CommandNotExistsException(cmdTypeName);

            var newCommand = (Command) Activator.CreateInstance(cmdType);

            Commands1.Add(command, new KeyValuePair<int, Command>(0, newCommand));

            newCommand.AddControl(control);

            return newCommand;
        }

        public static Command GetInstance<T>()
        {
            string[] commandName = typeof(T).Name.Split('.');
            return GetInstance(commandName[commandName.Length - 1]);
        }

        public static Command GetInstance<T>(IActiveControl control)
        {
            string[] commandName = typeof(T).Name.Split('.');
            return GetInstance(commandName[commandName.Length - 1], control);
        }

        public abstract void Run();

        private readonly List<IActiveControl> _assignedControls = new List<IActiveControl>();

        public void AddControl(IActiveControl control)
        {

            if (control == null)
                return;

            if (!_assignedControls.Contains(control))
                _assignedControls.Add(control);
        }

        public void FlushControls()
        {
            _assignedControls.Clear();
        }

        public bool Enabled
        {
            get
            {
                return _assignedControls.Select(control => control.ActiveEnabled).FirstOrDefault();
            }

            set
            {
                foreach (var control in _assignedControls)
                    control.ActiveEnabled = value;
            }
        }

        public bool Checked
        {
            get
            {
                return _assignedControls.Select(control => control.ActiveChecked).FirstOrDefault();
            }

            set
            {
                foreach (var control in _assignedControls)
                    control.ActiveChecked = value;
            }
        }
    }
}
