// LetterTileTk2DInput.cs
// Copyright (c) 2011-2016 Thinksquirrel Inc.
using System.Reflection;
using UnityEngine;
using Thinksquirrel.WordGameBuilder.ObjectModel;
using Thinksquirrel.WordGameBuilder.Internal;

#if UNITY_WINRT && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using System.Threading.Tasks;
#endif

namespace Thinksquirrel.WordGameBuilder.Tiles.Tk2D
{
    /// <summary>
    /// Adds input control to a letter tile, using 2D Toolkit's UI events.
    /// </summary>
    /// <remarks>
    /// This component must be on the same object as a letter tile.
    /// </remarks>
    [RequireComponent(typeof(ILetterTileInput))]
    [AddComponentMenu("Word Game Builder/Tiles/2D Toolkit/Letter Tile 2D Toolkit Input")]
    [WGBDocumentationName("Thinksquirrel.WordGameBuilder.Tiles.Tk2D.LetterTileTk2DInput")]
    public sealed class LetterTileTk2DInput : WGBBase
    {
        [SerializeField] MonoBehaviour m_UiItem;

        /// <summary>
        /// Gets or sets the UI item.
        /// </summary>
        /// <remarks>
        /// The UI item must be of the type tk2dUIItem.
        /// </remarks>
        public MonoBehaviour uiItem { get { return m_UiItem; } set { if (m_UiItem != value) { UnsubscribeFromEvents(); m_UiItem = value; SubscribeToEvents(); } } }

        ILetterTileInput m_LetterTile;

        EventInfo m_OnDown;
        EventInfo m_OnRelease;
        EventInfo m_OnHoverOver;
        EventInfo m_OnHoverOut;
        EventInfo m_OnClick;

        System.Action m_OnDownAction;
        System.Action m_OnReleaseAction;
        System.Action m_OnHoverOverAction;
        System.Action m_OnHoverOutAction;
        System.Action m_OnClickAction;

#if UNITY_WINRT && !UNITY_EDITOR
        async Task BindComponents()
#else
        void BindComponents()
#endif
        {
#if UNITY_WINRT && !UNITY_EDITOR
            var assemblyList = new List<Assembly>();
            var folder = Package.Current.InstalledLocation;
            foreach (var file in await folder.GetFilesAsync())
            {
                if (file.FileType == ".dll")
                {
                    var assemblyName = new AssemblyName(file.DisplayName);
                    var assembly = Assembly.Load(assemblyName);
                    assemblyList.Add(assembly);
                }
            }
            var assemblies = assemblyList.ToArray();
#else
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
#endif

            bool foundItem = false;
        
            for(int i = 0; i < assemblies.Length; ++i)
            {
                if (foundItem)
                    break;

                var assembly = assemblies[i];

                if (!foundItem)
                {
                    var controlType = assembly.GetType("tk2dUIItem");

                    if (controlType != null && controlType.Namespace == null)
                    {
                        m_OnDown = controlType.ExtGetEvent("OnDown");
                        m_OnRelease = controlType.ExtGetEvent("OnRelease");
                        m_OnHoverOver = controlType.ExtGetEvent("OnHoverOver");
                        m_OnHoverOut = controlType.ExtGetEvent("OnHoverOut");
                        m_OnClick = controlType.ExtGetEvent("OnClick");

                        foundItem = m_OnDown != null && m_OnRelease != null && m_OnHoverOver != null && m_OnHoverOut != null && m_OnClick != null;
                    }
                }
            }

            m_OnDownAction = Tk2DOnDown;
            m_OnReleaseAction = Tk2DOnRelease;
            m_OnHoverOverAction = Tk2DOnHoverOver;
            m_OnHoverOutAction = Tk2DOnHoverOut;
            m_OnClickAction = Tk2DOnClick;
        }

        void OnEnable()
        {
#if UNITY_WINRT && !UNITY_EDITOR
            var task = BindComponents();
            task.Wait();
#else
            BindComponents();
#endif
            m_LetterTile = GetComponentFromInterface<ILetterTileInput>();
            SubscribeToEvents();
        }

        void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        void SubscribeToEvents()
        {
            if (!enabled)
                return;

            if (!m_UiItem || m_UiItem == null)
                return;

            if (m_OnDown != null)
                m_OnDown.AddEventHandler(m_UiItem, m_OnDownAction);

            if (m_OnRelease != null)
                m_OnRelease.AddEventHandler(m_UiItem, m_OnReleaseAction);

            if (m_OnHoverOver != null)
                m_OnHoverOver.AddEventHandler(m_UiItem, m_OnHoverOverAction);

            if (m_OnHoverOut != null)
                m_OnHoverOut.AddEventHandler(m_UiItem, m_OnHoverOutAction);

            if (m_OnClick != null)
                m_OnClick.AddEventHandler(m_UiItem, m_OnClickAction);
        }

        void UnsubscribeFromEvents()
        {
            if (!m_UiItem || m_UiItem == null)
                return;

            if (m_OnDown != null)
                m_OnDown.RemoveEventHandler(m_UiItem, m_OnDownAction);

            if (m_OnRelease != null)
                m_OnRelease.RemoveEventHandler(m_UiItem, m_OnReleaseAction);

            if (m_OnHoverOver != null)
                m_OnHoverOver.RemoveEventHandler(m_UiItem, m_OnHoverOutAction);

            if (m_OnHoverOut != null)
                m_OnHoverOut.RemoveEventHandler(m_UiItem, m_OnHoverOutAction);

            if (m_OnClick != null)
                m_OnClick.RemoveEventHandler(m_UiItem, m_OnClickAction);
        }

        void Tk2DOnDown()
        {
            if (m_LetterTile != null && m_LetterTile.isActive && m_LetterTile.enabled)
            {
                m_LetterTile.SimulatePressInput(true);
            }
        }
        void Tk2DOnRelease()
        {
            if (m_LetterTile != null && m_LetterTile.isActive && m_LetterTile.enabled)
            {
                m_LetterTile.SimulatePressInput(false);
            }
        }
        void Tk2DOnHoverOver()
        {
            if (m_LetterTile != null && m_LetterTile.isActive && m_LetterTile.enabled)
            {
                m_LetterTile.SimulateHoverInput(true);
            }
        }
        void Tk2DOnHoverOut()
        {
            if (m_LetterTile != null && m_LetterTile.isActive && m_LetterTile.enabled)
            {
                m_LetterTile.SimulateHoverInput(false);
            }
        }
        void Tk2DOnClick()
        {
            if (m_LetterTile != null && m_LetterTile.isActive && m_LetterTile.enabled)
            {
                m_LetterTile.SimulateClickInput();
            }
        }
    }
}
