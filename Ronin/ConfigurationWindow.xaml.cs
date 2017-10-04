using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Logic;
using Ronin.Logic.Handlers;
using Ronin.Network;
using Ronin.Utilities;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using DragEventHandler = System.Windows.DragEventHandler;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;
using Orientation = System.Windows.Controls.Orientation;
using TextBox = System.Windows.Controls.TextBox;

namespace Ronin
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow
    {
        public static bool Opened;
        public static ConfigurationWindow Instance;

        private static readonly log4net.ILog log = LogHelper.GetLogger();

        public ConfigurationWindow(object dataContext)
        {
            DataContext = dataContext;
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Application.Current.DispatcherUnhandledException += (o, args) =>
            {
                Exception ex = (Exception) args.Exception;
                log.Debug("UI exception: ");
                log.Debug(ex);
                throw ex;
            };

            InitializeComponent();
            
            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseMoveEvent,
                new MouseEventHandler(NukeRulesListBox_PreviewMouseMove)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent,
                new DragEventHandler(NukeRulesListBox_Drop)));
            NukeRulesListBox.ItemContainerStyle = itemContainerStyle;

            itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseMoveEvent,
                new MouseEventHandler(NukeRulesListBox_PreviewMouseMove)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent,
                new DragEventHandler(SelfHealBuffRulesListBox_Drop)));
            SelfHealRulesListBox.ItemContainerStyle = itemContainerStyle;

            itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseMoveEvent,
                new MouseEventHandler(NukeRulesListBox_PreviewMouseMove)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent,
                new DragEventHandler(PartyHealBuffRulesListBox_Drop)));
            PartyHealRulesListBox.ItemContainerStyle = itemContainerStyle;

            Opened = true;
            Instance = this;
        }

        private void OpenMonsterFilter_txtBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot?.Engine.NukeHandler.SelectedNuke == null)
                return;

            OpenSelectionForm(((ViewModel)this.DataContext).SelectedBot?.Engine?.NukeHandler?.SelectedNuke?.MonsterFilter,
               value => MainWindow.ViewModel.SelectedBot.Engine.NukeHandler.SelectedNuke.MonsterFilterStr = value);
        }

        private void OpenTargetFilterIgnoreMonsterFilter_txtBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ViewModel)this.DataContext).SelectedBot?.Engine.Initialized != true)
                return;

            OpenSelectionForm(((ViewModel)this.DataContext).SelectedBot?.Engine?.MonsterFilter,
               value => MainWindow.ViewModel.SelectedBot.Engine.MonsterFilterStr = value);
        }

        private void OpenPlayerAssistFilter_txtBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot?.Engine?.AssistHandler.Initialiased != true)
                return;

            OpenSelectionForm(((ViewModel)this.DataContext).SelectedBot?.Engine?.AssistHandler?.SelectedPlayersFilter,
               value => MainWindow.ViewModel.SelectedBot.Engine.AssistHandler.SelectedPlayersStr = value);
        }

        private void OpenBuffSelection_txtBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot?.Engine.SelfHealBuffHandler.SelectedRule == null)
                return;

            OpenSelectionForm(((ViewModel)this.DataContext).SelectedBot?.Engine?.SelfHealBuffHandler?.SelectedRule?.SelectedBuffsFilter,
                value => MainWindow.ViewModel.SelectedBot.Engine.SelfHealBuffHandler.SelectedRule.SelectedBuffsStr = value);
        }

        private void OpenPlayerFollowSelection_txtBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot?.Engine.FollowHandler.Initialiased != true)
                return;

            OpenSelectionForm(((ViewModel)this.DataContext).SelectedBot?.Engine?.FollowHandler?.SelectedPlayersFilter,
                value => ((ViewModel)this.DataContext).SelectedBot.Engine.FollowHandler.SelectedPlayersStr = value);
        }

        private void OpenPlayerInviteSelection_txtBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ViewModel)this.DataContext).SelectedBot?.Engine?.DialogHandler.Initialiased != true)
                return;

            OpenSelectionForm(((ViewModel)this.DataContext).SelectedBot?.Engine?.DialogHandler?.SelectedPlayersToInviteFilter,
                value => ((ViewModel)this.DataContext).SelectedBot.Engine.DialogHandler.PlayersToInviteStr = value);
        }

        private void OpenPlayerAcceptPartyFromSelection_txtBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ViewModel)this.DataContext).SelectedBot?.Engine?.DialogHandler.Initialiased != true)
                return;

            OpenSelectionForm(((ViewModel)this.DataContext).SelectedBot?.Engine?.DialogHandler?.SelectedPlayersToAcceptPartyFilter,
                value => ((ViewModel)this.DataContext).SelectedBot.Engine.DialogHandler.PlayersToAcceptPartyStr = value);
        }

        private void OpenPlayerAcceptResFromSelection_txtBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ViewModel)this.DataContext).SelectedBot?.Engine?.DialogHandler.Initialiased != true)
                return;

            OpenSelectionForm(((ViewModel)this.DataContext).SelectedBot?.Engine?.DialogHandler?.SelectedPlayersToAcceptRessFilter,
                value => ((ViewModel)this.DataContext).SelectedBot.Engine.DialogHandler.PlayersToAcceptRessStr = value);
        }

        private void OpenSelectionForm(IEnumerable src, Action<string> setOutput)
        {
            var unitsForm = new SurroundingUnits();
            unitsForm.DataContext = this.DataContext;
            string listBoxString =
                @"<ListBox Height=""230""
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" >
                                <ListBox.ItemTemplate>
                                    <DataTemplate>
                                        <Grid>
                                            <Grid.ColumnDefinitions>
                                                <ColumnDefinition Width=""50""></ColumnDefinition>
                                                <ColumnDefinition></ColumnDefinition>
                                            </Grid.ColumnDefinitions>
                                            <CheckBox Grid.Column=""0"" IsChecked=""{Binding Enable, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"" HorizontalAlignment=""Center""/>
                                            <TextBlock HorizontalAlignment=""Center"" Grid.Column=""2"" Text=""{Binding Name}"" FontSize=""16"" FontStyle=""Italic""></TextBlock>
                                        </Grid>
                                    </DataTemplate>
                                </ListBox.ItemTemplate>
                            </ListBox>";
            var mainStackPanel = new StackPanel();
            ListBox listBox = (ListBox)XamlReader.Parse(listBoxString);
            mainStackPanel.Children.Add(listBox);
            listBox.ItemsSource = src;
            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Button addButton =
                (Button)
                XamlReader.Parse(
                    @"<Button  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Content=""Save"" FontSize=""14""/>");
            addButton.Click += (o, args) =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (UIFormElement buff in listBox.ItemsSource)
                {
                    if (buff.Enable)
                    {
                        sb.Append(buff.Name + ";");
                    }
                }

                setOutput(sb.ToString());
                unitsForm.Close();
            };

            stackPanel.Children.Add(addButton);

            Button closeButton =
                (Button)
                XamlReader.Parse(
                    @"<Button  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Content=""Close"" FontSize=""14"" Margin=""20,0,0,0"" />");
            closeButton.Click += (o, args) =>
            {
                unitsForm.Close();
            };

            stackPanel.Children.Add(closeButton);

            mainStackPanel.Children.Add(stackPanel);
            unitsForm.grid.Children.Add(mainStackPanel);
            Dispatcher.BeginInvoke((Action)(() => unitsForm.ShowDialog()));
        }

        private void OpenRemoveBuffSelection_txtBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot?.Engine?.SelfHealBuffHandler?.Initialiased != true)
                return;

            OpenSelectionForm(((ViewModel)this.DataContext).SelectedBot?.Engine?.SelfHealBuffHandler?.SelectedRemovalBuffsFilter,
               value => MainWindow.ViewModel.SelectedBot.Engine.SelfHealBuffHandler.SelectedRemovalBuffsStr = value);
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            Opened = false;
        }

        private void TwoDigitValidation_TbTextChange(object sender, TextChangedEventArgs e)
        {
            var txtBox = (TextBox) sender;
            txtBox.Text = Regex.Replace(txtBox.Text, "[^0-9]+", String.Empty);
            if (txtBox.Text.Length > 2)
            {
                txtBox.Text = txtBox.Text.Substring(1, 2);
            }

            txtBox.CaretIndex = txtBox.Text.Length;
        }

        private void OnlyDigitsValidationTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = (TextBox) sender;
            txtBox.Text = Regex.Replace(txtBox.Text, "[^0-9]+", String.Empty);
        }

        private void CommitChanges_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = ((TextBox) sender).GetBindingExpression(TextBox.TextProperty);
                exp.UpdateSource();
            }
        }

        private void AddNuke_ButtonClick(object sender, RoutedEventArgs e)
        {
            int index = skillsBox.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show("Please choose a skill first.");
                return;
            }

            if (MainWindow.ViewModel.SelectedBot != null)
            {
                var nukeRef = MainWindow.ViewModel.SelectedBot.Engine.NukeHandler.SelectedNuke;
                int nukeId =
                    MainWindow.ViewModel.SelectedBot?.Engine?.NukeHandler?.NukesToAdd[index].Id;
                nukeRef.NukeId = nukeId;
                nukeRef.Name = MainWindow.ViewModel.SelectedBot?.Engine?.NukeHandler?.NukesToAdd[index].SkillName;
                MainWindow.ViewModel.SelectedBot?.Engine?.NukeHandler?.NukesToUse.Add(nukeRef);
            }
        }

        private void AddSelfHealBuffRule_ButtonClick(object sender, RoutedEventArgs e)
        {
            int index = selfHealBuffBox.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show("Please choose a skill first.");
                return;
            }

            if (MainWindow.ViewModel.SelectedBot != null)
            {
                var nukeRef = MainWindow.ViewModel.SelectedBot.Engine.SelfHealBuffHandler.SelectedRule;
                int nukeId =
                    MainWindow.ViewModel.SelectedBot?.Engine?.SelfHealBuffHandler?.NukesToAdd[index].Id;
                nukeRef.SelfHealBuffId = nukeId;
                nukeRef.Name =
                    MainWindow.ViewModel.SelectedBot?.Engine?.SelfHealBuffHandler?.NukesToAdd[index].SkillName;
                MainWindow.ViewModel.SelectedBot?.Engine?.SelfHealBuffHandler?.NukesToUse.Add(nukeRef);
            }
        }

        private void AddPartyHealBuffRule_ButtonClick(object sender, RoutedEventArgs e)
        {
            int index = partyHealBuffBox.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show("Please choose a skill first.");
                return;
            }

            if (MainWindow.ViewModel.SelectedBot != null)
            {
                var nukeRef = MainWindow.ViewModel.SelectedBot.Engine.PartyHealBuffHandler.SelectedRule;
                int nukeId =
                    MainWindow.ViewModel.SelectedBot?.Engine?.PartyHealBuffHandler?.NukesToAdd[index].Id;
                nukeRef.PartyHealBuffId = nukeId;
                nukeRef.Name =
                    MainWindow.ViewModel.SelectedBot?.Engine?.PartyHealBuffHandler?.NukesToAdd[index].SkillName;
                MainWindow.ViewModel.SelectedBot?.Engine?.PartyHealBuffHandler?.NukesToUse.Add(nukeRef);
            }
        }

        private void SkillsBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot != null && skillsBox.SelectedIndex != -1)
            {
                NukeRulesListBox.SelectedIndex = -1;
                var newNuke = new NukeRule(((ViewModel) this.DataContext).SelectedBot.PlayerData)
                {
                    Enabled = true,
                    NukeType = (NukeType) ((ViewModel) this.DataContext).SelectedBot.Engine.NukeHandler.NukeTypeToAdd
                };

                int index = skillsBox.SelectedIndex;
                int nukeId = ((MultiThreadObservableCollection<dynamic>) skillsBox.ItemsSource)[index].Id;
                newNuke.NukeId = nukeId;
                ((ViewModel) this.DataContext).SelectedBot.Engine.NukeHandler.SelectedNuke = newNuke;
            }
        }

        private void SelfHealBuffBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot != null && selfHealBuffBox.SelectedIndex != -1)
            {
                SelfHealRulesListBox.SelectedIndex = -1;
                var newNuke = new SelfHealBuffRule(((ViewModel) this.DataContext).SelectedBot.PlayerData)
                {
                    Enabled = true,
                    NukeType =
                        (NukeType) ((ViewModel) this.DataContext).SelectedBot.Engine.SelfHealBuffHandler.NukeTypeToAdd
                };

                int index = selfHealBuffBox.SelectedIndex;
                int nukeId = ((MultiThreadObservableCollection<dynamic>) selfHealBuffBox.ItemsSource)[index].Id;
                newNuke.SelfHealBuffId = nukeId;
                ((ViewModel) this.DataContext).SelectedBot.Engine.SelfHealBuffHandler.SelectedRule = newNuke;
            }
        }

        private void PartyHealBuffBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot != null && partyHealBuffBox.SelectedIndex != -1)
            {
                PartyHealRulesListBox.SelectedIndex = -1;
                var newNuke = new PartyHealBuffRule(((ViewModel) this.DataContext).SelectedBot.PlayerData)
                {
                    Enabled = true,
                    NukeType =
                        (NukeType) ((ViewModel) this.DataContext).SelectedBot.Engine.PartyHealBuffHandler.NukeTypeToAdd
                };

                int index = partyHealBuffBox.SelectedIndex;
                int nukeId = ((MultiThreadObservableCollection<dynamic>) partyHealBuffBox.ItemsSource)[index].Id;
                newNuke.PartyHealBuffId = nukeId;
                ((ViewModel) this.DataContext).SelectedBot.Engine.PartyHealBuffHandler.SelectedRule = newNuke;
            }
        }

        private void NukeRulesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = NukeRulesListBox.SelectedIndex;
            if (index > -1 && skillsBox != null)
            {
                skillsBox.SelectedIndex = -1;
            }

            NukeRulesListBox.SelectedIndex = index;
        }

        private void SelfHealBuffRulesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = SelfHealRulesListBox.SelectedIndex;
            if (index > -1 && selfHealBuffBox!=null)
            {
                selfHealBuffBox.SelectedIndex = -1;
            }

            SelfHealRulesListBox.SelectedIndex = index;
        }

        private void PartyHealBuffRulesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = PartyHealRulesListBox.SelectedIndex;
            if (index > -1 && partyHealBuffBox!=null)
            {
                partyHealBuffBox.SelectedIndex = -1;
            }

            PartyHealRulesListBox.SelectedIndex = index;
        }

        private void NukeRulesListBox_Drop(object sender, DragEventArgs e)
        {
            NukeRule droppedData = e.Data.GetData(typeof(NukeRule)) as NukeRule;
            NukeRule target = ((ListBoxItem) (sender)).DataContext as NukeRule;

            int removedIdx = NukeRulesListBox.Items.IndexOf(droppedData);
            int targetIdx = NukeRulesListBox.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                MainWindow.ViewModel.SelectedBot.Engine.NukeHandler.NukesToUse.Insert(targetIdx + 1, droppedData);
                MainWindow.ViewModel.SelectedBot.Engine.NukeHandler.NukesToUse.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (MainWindow.ViewModel.SelectedBot.Engine.NukeHandler.NukesToUse.Count + 1 > remIdx)
                {
                    MainWindow.ViewModel.SelectedBot.Engine.NukeHandler.NukesToUse.Insert(targetIdx, droppedData);
                    MainWindow.ViewModel.SelectedBot.Engine.NukeHandler.NukesToUse.RemoveAt(remIdx);
                }
            }
        }

        private void SelfHealBuffRulesListBox_Drop(object sender, DragEventArgs e)
        {
            SelfHealBuffRule droppedData = e.Data.GetData(typeof(SelfHealBuffRule)) as SelfHealBuffRule;
            SelfHealBuffRule target = ((ListBoxItem) (sender)).DataContext as SelfHealBuffRule;

            int removedIdx = SelfHealRulesListBox.Items.IndexOf(droppedData);
            int targetIdx = SelfHealRulesListBox.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                MainWindow.ViewModel.SelectedBot.Engine.SelfHealBuffHandler.NukesToUse.Insert(targetIdx + 1, droppedData);
                MainWindow.ViewModel.SelectedBot.Engine.SelfHealBuffHandler.NukesToUse.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (MainWindow.ViewModel.SelectedBot.Engine.SelfHealBuffHandler.NukesToUse.Count + 1 > remIdx)
                {
                    MainWindow.ViewModel.SelectedBot.Engine.SelfHealBuffHandler.NukesToUse.Insert(targetIdx, droppedData);
                    MainWindow.ViewModel.SelectedBot.Engine.SelfHealBuffHandler.NukesToUse.RemoveAt(remIdx);
                }
            }
        }

        private void PartyHealBuffRulesListBox_Drop(object sender, DragEventArgs e)
        {
            PartyHealBuffRule droppedData = e.Data.GetData(typeof(PartyHealBuffRule)) as PartyHealBuffRule;
            PartyHealBuffRule target = ((ListBoxItem)(sender)).DataContext as PartyHealBuffRule;

            int removedIdx = PartyHealRulesListBox.Items.IndexOf(droppedData);
            int targetIdx = PartyHealRulesListBox.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                MainWindow.ViewModel.SelectedBot.Engine.PartyHealBuffHandler.NukesToUse.Insert(targetIdx + 1, droppedData);
                MainWindow.ViewModel.SelectedBot.Engine.PartyHealBuffHandler.NukesToUse.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (MainWindow.ViewModel.SelectedBot.Engine.PartyHealBuffHandler.NukesToUse.Count + 1 > remIdx)
                {
                    MainWindow.ViewModel.SelectedBot.Engine.PartyHealBuffHandler.NukesToUse.Insert(targetIdx, droppedData);
                    MainWindow.ViewModel.SelectedBot.Engine.PartyHealBuffHandler.NukesToUse.RemoveAt(remIdx);
                }
            }
        }

        private void NukeRulesListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is ListBoxItem && e.LeftButton == MouseButtonState.Pressed)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void DeleteNuke_ButtonClick(object sender, RoutedEventArgs e)
        {
            int index = NukeRulesListBox.SelectedIndex;
            if (index < 0)
                return;

            ((ViewModel) this.DataContext).SelectedBot.Engine.NukeHandler.NukesToUse.RemoveAt(index);
        }

        private void DeleteSelfHealBuffRule_ButtonClick(object sender, RoutedEventArgs e)
        {
            int index = SelfHealRulesListBox.SelectedIndex;
            if (index < 0)
                return;

            ((ViewModel) this.DataContext).SelectedBot.Engine.SelfHealBuffHandler.NukesToUse.RemoveAt(index);
        }

        private void DeletePartyHealBuffRule_ButtonClick(object sender, RoutedEventArgs e)
        {
            int index = PartyHealRulesListBox.SelectedIndex;
            if (index < 0)
                return;

            ((ViewModel) this.DataContext).SelectedBot.Engine.PartyHealBuffHandler.NukesToUse.RemoveAt(index);
        }

        private void OpenPartyTargetsSelect_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot?.Engine.PartyHealBuffHandler.SelectedRule == null)
                return;

            var unitsForm = new SurroundingUnits();
            unitsForm.DataContext = this.DataContext;
                //xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            string listBoxString =
                @"<ListBox Height=""230""
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" >
                                <ListBox.ItemTemplate>
                                    <DataTemplate>
                                        <Grid>
                                            <Grid.ColumnDefinitions>
                                                <ColumnDefinition Width=""50""></ColumnDefinition>
                                                <ColumnDefinition></ColumnDefinition>
                                            </Grid.ColumnDefinitions>
                                            <CheckBox Grid.Column=""0"" IsChecked=""{Binding Enable, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"" HorizontalAlignment=""Center""/>
                                            <TextBlock HorizontalAlignment=""Center"" Grid.Column=""2"" Text=""{Binding Name}"" FontSize=""16"" FontStyle=""Italic""></TextBlock>
                                        </Grid>
                                    </DataTemplate>
                                </ListBox.ItemTemplate>
                            </ListBox>";
            var mainStackPanel = new StackPanel();
            ListBox listBox = (ListBox) XamlReader.Parse(listBoxString);
            mainStackPanel.Children.Add(listBox);
            listBox.ItemsSource =
                ((ViewModel) this.DataContext).SelectedBot?.Engine?.PartyHealBuffHandler?.SelectedRule?.PlayersToBuff;
            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Button addButton =
                (Button)
                XamlReader.Parse(
                    @"<Button  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Content=""Save"" FontSize=""14""/>");
            addButton.Click += (o, args) =>
            {
                var curRule = MainWindow.ViewModel.SelectedBot.Engine.PartyHealBuffHandler.SelectedRule;
                curRule.PlayersFilter.Clear();
                foreach (UIFormElement partyUnit in listBox.ItemsSource)
                {
                    if (!partyUnit.Name.Contains("'s"))
                    {
                        if (partyUnit.Enable && !curRule.UseOnPlayer)
                            curRule.PlayersFilter.Add(partyUnit.Name, FilterType.Inclusive);
                        else if (!partyUnit.Enable && curRule.UseOnPlayer)
                            curRule.PlayersFilter.Add(partyUnit.Name, FilterType.Exclusive);
                    }
                    else
                    {
                        if (partyUnit.Enable && !curRule.UseOnSummonPet)
                            curRule.PlayersFilter.Add(partyUnit.Name, FilterType.Inclusive);
                        else if (!partyUnit.Enable && curRule.UseOnSummonPet)
                            curRule.PlayersFilter.Add(partyUnit.Name, FilterType.Exclusive);
                    }
                }

                unitsForm.Close();
            };

            stackPanel.Children.Add(addButton);

            Button closeButton =
                (Button)
                XamlReader.Parse(
                    @"<Button  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Content=""Close"" FontSize=""14"" Margin=""20,0,0,0"" />");
            closeButton.Click += (o, args) =>
            {
                unitsForm.Close();
            };

            stackPanel.Children.Add(closeButton);

            mainStackPanel.Children.Add(stackPanel);
            unitsForm.grid.Children.Add(mainStackPanel);
            Dispatcher.BeginInvoke((Action) (() => unitsForm.ShowDialog()));
        }

        private void GetCurrentLoc_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (MainWindow.ViewModel.SelectedBot == null)
                return;

            MainWindow.ViewModel.SelectedBot.Engine.PointX = MainWindow.ViewModel.SelectedBot.PlayerData.MainHero.X;
            MainWindow.ViewModel.SelectedBot.Engine.PointY = MainWindow.ViewModel.SelectedBot.PlayerData.MainHero.Y;
            MainWindow.ViewModel.SelectedBot.Engine.PointZ = MainWindow.ViewModel.SelectedBot.PlayerData.MainHero.Z;
        }

        private void OpenAdditionalNukeConditions_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (AdditionalNukeCondtions.Instance != null ||
                ((ViewModel) this.DataContext)?.SelectedBot?.Engine?.NukeHandler?.SelectedNuke == null) //
                return;

            var unitsForm = new AdditionalNukeCondtions();
            unitsForm.DataContext = this.DataContext;
            Dispatcher.BeginInvoke((Action) (() => unitsForm.ShowDialog()));
        }

        private void OpenAdditionalSelfConditions_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (AdditionalSelfCondtions.Instance != null ||
                ((ViewModel) this.DataContext)?.SelectedBot?.Engine?.SelfHealBuffHandler?.SelectedRule == null) //
                return;

            var unitsForm = new AdditionalSelfCondtions();
            unitsForm.DataContext = this.DataContext;
            Dispatcher.BeginInvoke((Action) (() => unitsForm.ShowDialog()));
        }

        private void OpenAdditionalPartyConditions_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (AdditionalPartyCondtions.Instance != null ||
                ((ViewModel) this.DataContext)?.SelectedBot?.Engine?.PartyHealBuffHandler?.SelectedRule == null) //
                return;

            var unitsForm = new AdditionalPartyCondtions();
            unitsForm.DataContext = this.DataContext;
            Dispatcher.BeginInvoke((Action) (() => unitsForm.ShowDialog()));
        }

        private void MetroWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                BindingExpression exp = ((TextBox)Keyboard.FocusedElement).GetBindingExpression(TextBox.TextProperty);
                exp?.UpdateSource();
                if (exp?.HasValidationError!=null && (bool)exp?.HasValidationError )
                    ((TextBox) Keyboard.FocusedElement).Text = "0";

                exp?.UpdateSource();
                Keyboard.ClearFocus();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public string GetWindowTitle()
        {
            string title = "";
            Dispatcher.Invoke((MethodInvoker)delegate {
                title = Title; // runs on UI thread
            });

            return title;
        }

        public void SetWindowTitle(string title)
        {
            Dispatcher.Invoke((MethodInvoker)delegate {
                Title = title; // runs on UI thread
            });
        }

        public void SetConfigurationBoxTitle(string title)
        {
            Dispatcher.Invoke((MethodInvoker)delegate {
                configurationCb.Text = title ?? string.Empty; // runs on UI thread
            });
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (((ViewModel)this.DataContext)?.SelectedBot?.Engine == null)
                return;

            if (!((ViewModel)this.DataContext).SelectedBot.Engine.Running)
                ((ViewModel)this.DataContext).SelectedBot.Engine.Start();
            else
            {
                ((ViewModel)this.DataContext).SelectedBot.Engine.Abort();
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            if (((ViewModel) this.DataContext).SelectedBot != null && ((ViewModel)this.DataContext).SelectedBot.PlayerData.MainHero.Name!=null)
            {
                var saveConfigurationForm = new SaveConfiguration(this.DataContext);
                Dispatcher.BeginInvoke((Action) (() => saveConfigurationForm.ShowDialog()));
            }
        }

        public void SaveConfiguration(string fileName)
        {
            Directory.CreateDirectory("Configurations/");
            File.WriteAllText(
                "Configurations/" + fileName +
                //L2BotsViewModel.SelectedBot.data.ServerId + 
                ".json",
                JsonConvert.SerializeObject(((ViewModel)this.DataContext).SelectedBot.Engine,
                Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }));
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ((ComboBox)sender).SelectedIndex;
            if (((ViewModel) DataContext).SelectedBot == null)
            {
                ((ComboBox)sender).SelectedIndex = -1;
                return;
            }

            if (index == -1)
            {
                return;
            }


            var curBot = ((ViewModel) DataContext).SelectedBot;
            if(curBot.SelectedConfiguration != ((ViewModel)DataContext).AvailableConfigurations[index])
                curBot.LoadConfiguration(((ViewModel)DataContext).AvailableConfigurations[index]);
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ((ViewModel)DataContext).ReloadConfigurations();
            ((ComboBox)sender).Text = ((ViewModel)DataContext).SelectedBot.SelectedConfiguration;
        }

        private void DebugWindowMonstersAround_Click(object sender, RoutedEventArgs e)
        {
            if (((ViewModel)this.DataContext).SelectedBot != null && ((ViewModel)this.DataContext).SelectedBot.PlayerData.MainHero.Name != null)
            {
                var dynamicNpcsAround = new DynamicNpcsAround(this.DataContext);
                Dispatcher.BeginInvoke((Action)(() => dynamicNpcsAround.Show()));
            }
        }

        private void SearchItemsPickupAdd_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (((ViewModel)this.DataContext).SelectedBot?.Engine?.PickupHandler.Initialiased != true)
                return;

            var unitsForm = new SurroundingUnits();
            unitsForm.DataContext = this.DataContext;
            //xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            string listBoxString =
                @"<ListBox Height=""230""
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" >
                                <ListBox.ItemTemplate>
                                    <DataTemplate>
                                        <Grid>
                                            <Grid.ColumnDefinitions>
                                                <ColumnDefinition Width=""50""></ColumnDefinition>
                                                <ColumnDefinition></ColumnDefinition>
                                            </Grid.ColumnDefinitions>
                                            <CheckBox Grid.Column=""0"" IsChecked=""{Binding Enable, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"" HorizontalAlignment=""Center""/>
                                            <TextBlock HorizontalAlignment=""Center"" Grid.Column=""2"" Text=""{Binding Name}"" FontSize=""16"" FontStyle=""Italic""></TextBlock>
                                        </Grid>
                                    </DataTemplate>
                                </ListBox.ItemTemplate>
                            </ListBox>";
            var mainStackPanel = new StackPanel();
            ListBox listBox = (ListBox)XamlReader.Parse(listBoxString);
            mainStackPanel.Children.Add(listBox);
            listBox.ItemsSource =
                ((ViewModel)this.DataContext).SelectedBot?.Engine?.PickupHandler?.FilteredItems;
            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Button addButton =
                (Button)
                XamlReader.Parse(
                    @"<Button  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Content=""Save"" FontSize=""14""/>");
            addButton.Click += (o, args) =>
            {
                var pickupHandler = ((ViewModel) DataContext).SelectedBot.Engine.PickupHandler;
                foreach (UIItemAdd item in listBox.ItemsSource)
                {
                    if (item.Enable)
                        pickupHandler.RulesInUse.Add(new PickupRule(item.Id));
                }

                unitsForm.Close();
            };

            stackPanel.Children.Add(addButton);

            Button closeButton =
                (Button)
                XamlReader.Parse(
                    @"<Button  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Content=""Close"" FontSize=""14"" Margin=""20,0,0,0"" />");
            closeButton.Click += (o, args) =>
            {
                unitsForm.Close();
            };

            stackPanel.Children.Add(closeButton);

            mainStackPanel.Children.Add(stackPanel);
            unitsForm.grid.Children.Add(mainStackPanel);
            Dispatcher.BeginInvoke((Action)(() => unitsForm.ShowDialog()));
        }

        private void RemovePickupRule_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (((ViewModel)this.DataContext).SelectedBot?.Engine?.PickupHandler.Initialiased != true)
                return;

            if (((ViewModel)this.DataContext).SelectedBot?.Engine?.PickupHandler.SelectedPickupRule == null)
                return;

            ((ViewModel) this.DataContext).SelectedBot?.Engine?.PickupHandler.RulesInUse.Remove(((ViewModel) this.DataContext).SelectedBot?.Engine?.PickupHandler.SelectedPickupRule);
        }

        private void TriggerItemSearchOnEnterDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchItemsPickupAdd_ButtonClick(sender, new RoutedEventArgs());
            }
        }
    }
}
