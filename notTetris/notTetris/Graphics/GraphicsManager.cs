﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace NotTetris.Graphics
{
    /// <summary>
    /// Static class handling loading of graphics content
    /// </summary>
    public static class GraphicsManager
    {
        private static string[] texturePaths = {
            @"Content\Textures\mouse_cursor",
            @"Content\Textures\button_increase",
            @"Content\Textures\game_background",
            @"Content\Textures\game_paused",
            @"Content\Textures\playfieldbackground_yellow",
            @"Content\Textures\block_red",
            @"Content\Textures\block_blue",
            @"Content\Textures\block_gray",
            @"Content\Textures\block_purple",
            @"Content\Textures\block_yellow",
            @"Content\Textures\block_green",
            @"Content\Textures\block_orange",
            @"Content\Textures\block_explosion",
            @"Content\Textures\mainmenu_background",
            @"Content\Textures\title",
            @"Content\Textures\red_line",
            @"Content\Textures\popup_background",
        };

        private static string[] fontPaths = {
            @"Content\Fonts\Segoe UI Mono",
            @"Content\Fonts\Segoe UI Mono - Large",
            @"Content\Fonts\Segoe UI Mono - Huge",
        };

        private static Texture2D[] loadedTextures;
        private static SpriteFont[] loadedFonts;

        public static Texture2D GetTexture(TextureNames texture)
        {
            return loadedTextures[(int)texture];
        }

        public static SpriteFont GetFont(FontNames font)
        {
            return loadedFonts[(int)font];
        }

        public static void LoadAllContent(ContentManager content)
        {
            loadedTextures = new Texture2D[texturePaths.Length];
            loadedFonts = new SpriteFont[fontPaths.Length];

            for (int i = 0; i < texturePaths.Length; i++)
                loadedTextures[i] = TryLoad<Texture2D>(content, texturePaths[i]);

            for (int i = 0; i < fontPaths.Length; i++)
                loadedFonts[i] = TryLoad<SpriteFont>(content, fontPaths[i]);
        }

        private static T TryLoad<T>(ContentManager content, string path)
        {
            T ret;
            try
            {
                ret = content.Load<T>(path);
            }
            catch
            {
                throw new Exception("Unable to load file at: " + path);
            }
            return ret;
        }
    }

    public enum TextureNames
    {
        mouse_cursor,
        button_increase,
        game_background,
        game_paused,
        playfieldbackground_yellow,
        block_red,
        block_blue,
        block_gray,
        block_purple,
        block_yellow,
        block_green,
        block_orange,
        block_explosion,
        mainmenu_background,
        title,
        red_line,
        popup_background,
    }

    public enum FontNames
    {
        Segoe_UI_Mono,
        Segoe_UI_Mono_Large,
        Segoe_UI_Mono_Huge,
    }

    public interface IDrawable
    {
        void LoadContent(SpriteBatch spriteBatch);
        void Draw(GameTime gameTime);
    }
}
