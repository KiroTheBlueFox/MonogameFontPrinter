using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace KiroTheBlueFox.Utils
{
    /// <summary>
    /// Made by KiroTheBlueFox.
    /// Free to use and modify.
    /// </summary>
    public class Font
    {
        private string filePath;
        private Texture2D texture;
        private FontParameters fontParameters;
        private Dictionary<char, Rectangle> chars;
        public readonly Game Game;
        public string Name { get => fontParameters.Name; }
        public int Height { get => fontParameters.Height; }

        public Font(Game game, string filePath)
        {
            Game = game;
            this.filePath = filePath;
        }

        public void Load()
        {
            texture = Game.Content.Load<Texture2D>(filePath);

            var paramsPath = Path.Combine(Game.Content.RootDirectory, filePath+".json");
            using (var stream = TitleContainer.OpenStream(paramsPath))
            {
                StreamReader reader = new StreamReader(stream);
                string json = reader.ReadToEnd();
                fontParameters = JsonConvert.DeserializeObject<FontParameters>(json);
            }

            if (fontParameters != null)
            {
                chars = new Dictionary<char, Rectangle>();
                for (int i = 0; i < fontParameters.Characters.Length; i++)
                {
                    for (int j = 0; j < fontParameters.Characters[i].Length; j++)
                    {
                        char character = fontParameters.Characters[i][j];
                        chars.Add(character, new Rectangle((i + fontParameters.MinimumCharacterWidth) * j,
                                                            i * fontParameters.Height,
                                                            i + fontParameters.MinimumCharacterWidth,
                                                            fontParameters.Height));
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, string text, Vector2 position, FontOptions fontOptions)
        {
            int currentWidth = 0;
            int currentHeight = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                if (chars.ContainsKey(character))
                {
                    spriteBatch.Draw(texture, position + (new Vector2(currentWidth, currentHeight) * fontOptions.Size), chars[character], fontOptions.Color, 0, Vector2.Zero, fontOptions.Size, SpriteEffects.None, 0);
                    currentWidth += chars[character].Width + fontOptions.CharSpacing;
                }
                else
                {
                    switch (character)
                    {
                        case ' ':
                            currentWidth += fontParameters.SpaceLength + fontOptions.CharSpacing;
                            break;
                        case '\t':
                            currentWidth += fontParameters.TabLength + fontOptions.CharSpacing;
                            break;
                        case '\n':
                            currentWidth = 0;
                            currentHeight += fontParameters.Height + fontOptions.LineSpacing;
                            break;
                        default:
                            char missingChar = fontParameters.MissingCharacter[0];
                            spriteBatch.Draw(texture, position + (new Vector2(currentWidth, currentHeight) * fontOptions.Size), chars[missingChar], fontOptions.Color, 0, Vector2.Zero, fontOptions.Size, SpriteEffects.None, 0);
                            currentWidth += chars[missingChar].Width + fontOptions.CharSpacing;
                            break;
                    }
                }
            }
        }

        public Vector2 TextSize(string text, FontOptions fontOptions)
        {
            List<int> widths = new List<int>();
            int height = fontParameters.Height + fontOptions.LineSpacing, line = 0;
            widths.Add(0);
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                if (chars.ContainsKey(character))
                {
                    widths[line] += chars[character].Width + fontOptions.CharSpacing;
                }
                else
                {
                    switch (character)
                    {
                        case ' ':
                            widths[line] += fontParameters.SpaceLength + fontOptions.CharSpacing;
                            break;
                        case '\t':
                            widths[line] += fontParameters.TabLength + fontOptions.CharSpacing;
                            break;
                        case '\n':
                            height += fontParameters.Height + fontOptions.LineSpacing;
                            line++;
                            widths.Add(0);
                            break;
                        default:
                            widths[line] += chars[fontParameters.MissingCharacter[0]].Width + fontOptions.CharSpacing;
                            break;
                    }
                }
            }
            int maxWidth = 0;
            foreach (int width in widths)
            {
                if (width > maxWidth)
                    maxWidth = width;
            }
            return new Vector2(maxWidth, height) * fontOptions.Size;
        }
    }

    class FontParameters
    {
        public string Name, MissingCharacter;
        public int Height, SpaceLength, TabLength, MinimumCharacterWidth;
        public string[] Characters;
    }

    public class FontOptions
    {
        /// <summary>
        /// Size multiplicator of the font. 1 by default.
        /// </summary>
        public readonly float Size;

        /// <summary>
        /// Color of the font. White by default
        /// </summary>
        public readonly Color Color;

        /// <summary>
        /// Separation between characters (in pixels). 0 by default.
        /// </summary>
        public readonly int CharSpacing;

        /// <summary>
        /// Separation between lines (in pixels). 0 by default.
        /// </summary>
        public readonly int LineSpacing;

        public FontOptions()
        {
            Size = 1;
            Color = Color.White;
            CharSpacing = 0;
            LineSpacing = 0;
        }

        public FontOptions(float size, Color color, int charSpacing, int lineSpacing)
        {
            Size = size;
            Color = color;
            CharSpacing = charSpacing;
            LineSpacing = lineSpacing;
        }
    }
