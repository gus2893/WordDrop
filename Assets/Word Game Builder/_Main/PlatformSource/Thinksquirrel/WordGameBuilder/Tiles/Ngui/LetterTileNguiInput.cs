// LetterTileNguiInput.cs
// Copyright (c) 2011-2016 Thinksquirrel Inc.
using UnityEngine;
using Thinksquirrel.WordGameBuilder.ObjectModel;
using Thinksquirrel.WordGameBuilder.Internal;

namespace Thinksquirrel.WordGameBuilder.Tiles.Ngui
{
    /// <summary>
    /// Adds input control to a letter tile, using NGUI's OnPress/OnHover/OnClick methods.
    /// </summary>
    /// <remarks>
    /// This component must be on the same object as a letter tile.
    /// </remarks>
    [RequireComponent(typeof(ILetterTileInput))]
    [AddComponentMenu("Word Game Builder/Tiles/NGUI/Letter Tile NGUI Input")]
    [WGBDocumentationName("Thinksquirrel.WordGameBuilder.Tiles.Ngui.LetterTileNguiInput")]
    public sealed class LetterTileNguiInput : WGBBase
    {
        ILetterTileInput m_LetterTile;

        void OnEnable()
        {
            m_LetterTile = GetComponentFromInterface<ILetterTileInput>();
        }

        void OnPress(bool isPressed)
        {
            if (m_LetterTile != null && m_LetterTile.isActive && m_LetterTile.enabled)
            {
                m_LetterTile.SimulatePressInput(isPressed);
            }
        }

        void OnHover(bool isOver)
        {
            if (m_LetterTile != null && m_LetterTile.isActive && m_LetterTile.enabled)
            {
                m_LetterTile.SimulateHoverInput(isOver);
            }
        }

        void OnClick()
        {
            if (m_LetterTile != null && m_LetterTile.isActive && m_LetterTile.enabled)
            {
                m_LetterTile.SimulateClickInput();
            }
        }
    }
}
