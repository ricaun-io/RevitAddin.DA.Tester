﻿using ricaun.Forge.DesignAutomation.Extensions;

namespace DesignAutomationConsole.Models
{
    public class InputModel
    {
        public string Text { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}