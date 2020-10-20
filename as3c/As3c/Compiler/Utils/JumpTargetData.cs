using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using As3c.Common;
using As3c.Compiler;

namespace as3c.As3c.Compiler.Utils
{
    public class JumpTargetData
    {
        public AVM2Command TargetCommand
        {
            get;
            set;
        }
		private List<AVM2Command> extraCommands;

        public void addTarget(AVM2Command targetCommand) {
			if (targetCommand == null) {
				return;
			}
			if(extraCommands==null){
                extraCommands = new List<AVM2Command>();
            }
			extraCommands.Add(targetCommand);
		}


        public List<AVM2Command> ExtraCommands
        {
			get {
                return extraCommands;
            }
		}

        public AVM2Command JumpCommand
        {
            get;
            private set;
        }

        public JumpTargetData(AVM2Command jumpCommand, AVM2Command targetCommand = null)
        {
            this.JumpCommand = jumpCommand;
            this.TargetCommand = targetCommand;
		}
    }
}
