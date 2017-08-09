﻿using System;
using Intersect.GameObjects;
using IntersectClientExtras.File_Management;
using IntersectClientExtras.GenericClasses;
using IntersectClientExtras.Graphics;
using IntersectClientExtras.Gwen.Control;
using IntersectClientExtras.Gwen.Control.EventArguments;
using IntersectClientExtras.Gwen.Input;
using IntersectClientExtras.Input;
using Intersect_Client.Classes.General;
using Intersect_Client.Classes.Networking;
using Intersect_Client.Classes.UI.Game;

namespace Intersect.Client.Classes.UI.Game.Bag
{
    public class BagItem
    {
        private static int ItemXPadding = 4;
        private static int ItemYPadding = 4;

        //Drag/Drop References
        private BagWindow _bagWindow;

        private int _currentItem = -2;
        private ItemDescWindow _descWindow;

        //Slot info
        private int _mySlot;

        //Dragging
        private bool CanDrag;

        private long ClickTime;
        public ImagePanel container;
        private Draggable dragIcon;
        public bool IsDragging;

        //Mouse Event Variables
        private bool MouseOver;

        private int MouseX = -1;
        private int MouseY = -1;
        public ImagePanel pnl;

        public BagItem(BagWindow bagWindow, int index)
        {
            _bagWindow = bagWindow;
            _mySlot = index;
        }

        public void Setup()
        {
            pnl = new ImagePanel(container, "BagItemIcon");
            pnl.HoverEnter += pnl_HoverEnter;
            pnl.HoverLeave += pnl_HoverLeave;
            pnl.RightClicked += pnl_RightClicked;
            pnl.DoubleClicked += Pnl_DoubleClicked;
            pnl.Clicked += pnl_Clicked;
        }

        private void Pnl_DoubleClicked(Base sender, ClickedEventArgs arguments)
        {
            if (Globals.InBag)
            {
                Globals.Me.TryRetreiveBagItem(_mySlot);
            }
        }

        void pnl_Clicked(Base sender, ClickedEventArgs arguments)
        {
            ClickTime = Globals.System.GetTimeMS() + 500;
        }

        void pnl_RightClicked(Base sender, ClickedEventArgs arguments)
        {
        }

        void pnl_HoverLeave(Base sender, EventArgs arguments)
        {
            MouseOver = false;
            MouseX = -1;
            MouseY = -1;
            if (_descWindow != null)
            {
                _descWindow.Dispose();
                _descWindow = null;
            }
        }

        void pnl_HoverEnter(Base sender, EventArgs arguments)
        {
            MouseOver = true;
            CanDrag = true;
            if (Globals.InputManager.MouseButtonDown(GameInput.MouseButtons.Left))
            {
                CanDrag = false;
                return;
            }
            if (_descWindow != null)
            {
                _descWindow.Dispose();
                _descWindow = null;
            }
            if (Globals.Bag[_mySlot] != null)
            {
                _descWindow = new ItemDescWindow(Globals.Bag[_mySlot].ItemNum, Globals.Bag[_mySlot].ItemVal,
                    _bagWindow.X - 255, _bagWindow.Y, Globals.Bag[_mySlot].StatBoost);
            }
        }

        public FloatRect RenderBounds()
        {
            FloatRect rect = new FloatRect()
            {
                X = pnl.LocalPosToCanvas(new IntersectClientExtras.GenericClasses.Point(0, 0)).X,
                Y = pnl.LocalPosToCanvas(new IntersectClientExtras.GenericClasses.Point(0, 0)).Y,
                Width = pnl.Width,
                Height = pnl.Height
            };
            return rect;
        }

        public void Update()
        {
            if (Globals.Bag[_mySlot].ItemNum != _currentItem)
            {
                _currentItem = Globals.Bag[_mySlot].ItemNum;
                var item = ItemBase.Lookup.Get<ItemBase>(Globals.Bag[_mySlot].ItemNum);
                if (item != null)
                {
                    GameTexture itemTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Item,
                        item.Pic);
                    if (itemTex != null)
                    {
                        pnl.Texture = itemTex;
                    }
                    else
                    {
                        if (pnl.Texture != null)
                        {
                            pnl.Texture = null;
                        }
                    }
                }
                else
                {
                    if (pnl.Texture != null)
                    {
                        pnl.Texture = null;
                    }
                }
            }
            if (!IsDragging)
            {
                if (MouseOver)
                {
                    if (!Globals.InputManager.MouseButtonDown(GameInput.MouseButtons.Left))
                    {
                        CanDrag = true;
                        MouseX = -1;
                        MouseY = -1;
                        if (Globals.System.GetTimeMS() < ClickTime)
                        {
                            //Globals.Me.TryUseItem(_mySlot);
                            ClickTime = 0;
                        }
                    }
                    else
                    {
                        if (CanDrag)
                        {
                            if (MouseX == -1 || MouseY == -1)
                            {
                                MouseX = InputHandler.MousePosition.X -
                                         pnl.LocalPosToCanvas(new IntersectClientExtras.GenericClasses.Point(0, 0)).X;
                                MouseY = InputHandler.MousePosition.Y -
                                         pnl.LocalPosToCanvas(new IntersectClientExtras.GenericClasses.Point(0, 0)).Y;
                            }
                            else
                            {
                                int xdiff = MouseX -
                                            (InputHandler.MousePosition.X -
                                             pnl.LocalPosToCanvas(new IntersectClientExtras.GenericClasses.Point(0, 0))
                                                 .X);
                                int ydiff = MouseY -
                                            (InputHandler.MousePosition.Y -
                                             pnl.LocalPosToCanvas(new IntersectClientExtras.GenericClasses.Point(0, 0))
                                                 .Y);
                                if (Math.Sqrt(Math.Pow(xdiff, 2) + Math.Pow(ydiff, 2)) > 5)
                                {
                                    IsDragging = true;
                                    dragIcon = new Draggable(
                                        pnl.LocalPosToCanvas(new IntersectClientExtras.GenericClasses.Point(0, 0)).X +
                                        MouseX,
                                        pnl.LocalPosToCanvas(new IntersectClientExtras.GenericClasses.Point(0, 0)).X +
                                        MouseY, pnl.Texture);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (dragIcon.Update())
                {
                    //Drug the item and now we stopped
                    IsDragging = false;
                    FloatRect dragRect = new FloatRect(dragIcon.X - ItemXPadding / 2, dragIcon.Y - ItemYPadding / 2,
                        ItemXPadding / 2 + 32, ItemYPadding / 2 + 32);

                    float bestIntersect = 0;
                    int bestIntersectIndex = -1;
                    //So we picked up an item and then dropped it. Lets see where we dropped it to.
                    //Check inventory first.
                    if (_bagWindow.RenderBounds().IntersectsWith(dragRect))
                    {
                        for (int i = 0; i < Globals.Bag.Length; i++)
                        {
                            if (_bagWindow.Items[i].RenderBounds().IntersectsWith(dragRect))
                            {
                                if (FloatRect.Intersect(_bagWindow.Items[i].RenderBounds(), dragRect).Width *
                                    FloatRect.Intersect(_bagWindow.Items[i].RenderBounds(), dragRect).Height >
                                    bestIntersect)
                                {
                                    bestIntersect =
                                        FloatRect.Intersect(_bagWindow.Items[i].RenderBounds(), dragRect).Width *
                                        FloatRect.Intersect(_bagWindow.Items[i].RenderBounds(), dragRect).Height;
                                    bestIntersectIndex = i;
                                }
                            }
                        }
                        if (bestIntersectIndex > -1)
                        {
                            if (_mySlot != bestIntersectIndex)
                            {
                                //Try to swap....
                                PacketSender.SendMoveBagItems(bestIntersectIndex, _mySlot);
                            }
                        }
                    }
                    dragIcon.Dispose();
                }
            }
        }
    }
}