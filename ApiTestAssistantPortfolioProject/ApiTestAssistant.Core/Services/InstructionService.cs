using System;
using System.Collections.Generic;
using System.Text;
using ApiTestAssistant.Core.Models;

namespace ApiTestAssistant.Core.Services
{
    public class InstructionService
    {
        public InstructionService()
        {
        }

        public InstructionSet LoadInstructions(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new InstructionSet { RawInstructions = string.Empty };
            }

            return new InstructionSet { RawInstructions = System.IO.File.ReadAllText(path) };
        }
    }
}
