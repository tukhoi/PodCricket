using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PodCricket.Utilities.Toolkit
{
    public class DownloadControl : TransferControl
    {
        /// <summary>
        /// Client should register to this to handle event when user clicks to "add to play" context menu item
        /// </summary>
        public event EventHandler AddToPlay;

        public override void OnApplyTemplate()
        {
            BindImage();
            BindContextMenuItem();
            BindContentControl();
            base.OnApplyTemplate();
        }

        private void BindImage()
        {
            var monitor = base.Monitor as DownloadMonitor;
            if (monitor == null || monitor.ImageUri == null) return;

            base.Icon = monitor.ImageUri;
        }

        private void BindContextMenuItem()
        {
            var rootBorder = GetTemplateChild("TransferControl");
            if (rootBorder == null) return;

            var oldContextMenu = ContextMenuService.GetContextMenu(rootBorder);
            if (oldContextMenu == null) return;

            //if (contextMenu.Items.Count > 0)
            //{
            //    var cancelMenuItem = contextMenu.Items[0] as MenuItem;
            //    if (cancelMenuItem == null) return;

            //    cancelMenuItem.Visibility = System.Windows.Visibility.Collapsed;
            //}

            oldContextMenu.Visibility = System.Windows.Visibility.Collapsed;

            var contextMenu = new ContextMenu();
            contextMenu.Visibility = System.Windows.Visibility.Visible;


            var newCancelMenuItem = new MenuItem();
            newCancelMenuItem.Name = "ContextMenuCancel2";
            newCancelMenuItem.Header = "cancel";
            newCancelMenuItem.Tap += (sender, args) => { if (base.Monitor != null) base.Monitor.RequestCancel(); };
            contextMenu.Items.Add(newCancelMenuItem);

            var addToPlayMenuItem = new MenuItem();
            addToPlayMenuItem.Name = "ContextMenuAdd";
            addToPlayMenuItem.Header = "add to play";
            addToPlayMenuItem.Tap += addToPlayAction_Tap;

            contextMenu.Items.Add(addToPlayMenuItem);

            ContextMenuService.SetContextMenu(rootBorder, contextMenu);
        }

        private void BindContentControl()
        {
            var monitor = base.Monitor as DownloadMonitor;
            if (monitor == null || monitor.ImageUri == null) return;

            var header = GetTemplateChild("Header") as ContentControl;
            if (header == null) return;

            header.Content = monitor.PodName;

            var stackPanel = GetTemplateChild("ListItem") as StackPanel;
            if (stackPanel == null) return;

            var textBlock = new TextBlock(){
                Text = monitor.StreamTitle,
                Style = (System.Windows.Style)Resources["PhoneTextSmallStyle"],
                Margin = new System.Windows.Thickness(0)
            };

            ToolTipService.SetToolTip(textBlock, monitor.StreamTitle);

            stackPanel.Children.Insert(1, textBlock);
        }

        void addToPlayAction_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AddToPlay(sender, e);
        }
    }
}
