using UnityEngine;

namespace Irehon.UI
{
    public struct TooltipMessage
    {
        public Color Color { get; set; }
        public string Message { get; set; }
        public int Font { get; set; }

        public TooltipMessage(Color color, string message, int font)
        {
            this.Color = color;
            this.Message = message;
            this.Font = font;
        }

        public TooltipMessage(Color color, string message)
        {
            this.Message = message;
            this.Color = color;
            this.Font = 14;
        }

        public TooltipMessage(string message, int font, int maxWordsInLine = 0)
        {
            this.Color = Color.white;
            this.Font = font;

            if (maxWordsInLine != 0)
            {
                string[] words = message.Split(' ');

                int wordCount = words.Length;

                int requiredLinesAmount = wordCount / maxWordsInLine;

                message = "";

                for (int i = 0; i < wordCount; i++)
                {
                    message += words[i] + " ";
                    if (i % 4 == 0 && i != 0 && i != wordCount - 1)
                    {
                        message += System.Environment.NewLine;
                    }
                }
            }

            this.Message = message;
        }

        public TooltipMessage(string message)
        {
            this.Color = Color.white;
            this.Font = 14;
            this.Message = message;
        }
    }
}