using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GachaSimulator.Models;
using GachaSimulatorGUI.Controllers;

namespace GachaSimulatorGUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly GameController _controller;
    
    public MainWindow()
    {
        InitializeComponent();
        _controller = new GameController();
        PlayerNameText.Text = _controller.CurrentPlayer.Name;
        UpdateCoinsDisplay();

        BoxesListBox.ItemsSource = _controller.Boxes;
        if (_controller.Boxes.Count > 0)
        {
            BoxesListBox.SelectedIndex = 0;
        }

        RefreshInventoryList();
    }

    private void UpdateCoinsDisplay()
    {
        CoinsText.Text = $"{_controller.CurrentPlayer.Coins} monet";
    }

    private void BoxesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BoxesListBox.SelectedItem is Blindbox selectedBox)
        {
            SelectedBoxTitle.Text = selectedBox.CategoryName;
            SelectedBoxCost.Text = $"Koszt otwarcia: {selectedBox.Cost} monet";
            PossibleItemList.ItemsSource = selectedBox.PossibleItems;
        }
    }

    private void InventoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (InventoryListBox.SelectedItem is Item item)
        {
            Brush rarityBrush = GetRarityBrush(item.ItemRarity);
            
            ItemsStatsCard.BorderBrush = rarityBrush;
            ItemRarityText.Foreground = rarityBrush;
            ItemRarityText.Text = item.ItemRarity.ToString().ToUpper();
            ItemNameText.Text = item.Name;
            ItemDescText.Text = $"\"{item.Description}\"";

            ItemAttackText.Text = $"Atak: {item.AttackValue} pkt";
            ItemAttackBar.Value = item.AttackValue;
            
            ItemDefenseText.Text = $"Obrona: {item.DefenseValue} pkt";
            ItemDefenseBar.Value = item.DefenseValue;
            
            ItemHealthText.Text = $"Życie: {item.HealthValue} pkt";
            ItemHealthBar.Value = item.HealthValue;

            ItemValueText.Text = $"Wartość sprzedaży: {item.MarketValue} monet";
            SellItemBtn.IsEnabled = true;
        }
        else
        {
            ItemsStatsCard.BorderBrush = new SolidColorBrush(Color.FromRgb(45, 51, 72));
            ItemRarityText.Text = "Brak postaci.";
            ItemRarityText.Foreground = Brushes.Gray;
            ItemNameText.Text = "Ekwipunek jest pusty";
            ItemNameText.Foreground = Brushes.White;
            ItemDescText.Text = "Otwórz pakiety w zakładce Pakiety, aby zdobyć swoje postacie!";
            
            ItemAttackText.Text = $"Atak: 0 pkt";
            ItemAttackBar.Value = 0;
            
            ItemDefenseText.Text = $"Obrona: 0 pkt";
            ItemDefenseBar.Value = 0;
            
            ItemHealthText.Text = $"Życie: 0 pkt";
            ItemHealthBar.Value = 0;

            ItemValueText.Text = $"Wartość sprzedaży: 0 monet";
            SellItemBtn.IsEnabled = false;
        }
    }

    private void RefreshInventoryList()
    {
        int previousIndex = InventoryListBox.SelectedIndex;
        InventoryListBox.ItemsSource = null;
        InventoryListBox.ItemsSource = _controller.CurrentPlayer.Inventory;
        InventoryHeaderCount.Text = $"Twoja kolekcja: ({_controller.CurrentPlayer.Inventory.Count})";

        if (_controller.CurrentPlayer.Inventory.Count > 0)
        {
            InventoryListBox.SelectedIndex = Math.Min(Math.Max(0,previousIndex), _controller.CurrentPlayer.Inventory.Count - 1);
        }
        else
        {
            InventoryListBox.SelectedIndex = -1;
        }
    }

    private void OpenBoxBtn_Click(object sender, RoutedEventArgs e)
    {
        if (BoxesListBox.SelectedItem is not Blindbox selectedBox)
            return;
        
        Item? droppedItem = _controller.OpenBox(selectedBox);

        if (droppedItem == null)
        {
            MessageBox.Show("Nie masz wystarczająco dużo monet! Sprzedaj postacie w ekwipunku (klawisz I), aby zdobyć złoto.",
                "Brak środków", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        UpdateCoinsDisplay();
        RefreshInventoryList();
        PlayBoxOpeningAnimation(selectedBox.CategoryName, droppedItem);
    }

    private void PlayBoxOpeningAnimation(string boxName, Item droppeditem)
    {
        AnimationOverlay.Visibility = Visibility.Visible;
        ShakingBoxVisual.Visibility = Visibility.Visible;
        RewardCardVisual.Visibility = Visibility.Collapsed;
        OpeningBoxLabel.Text = $"Otwieranie: {boxName}...";

        var shakenAnimation = new DoubleAnimation()
        {
            From = -14,
            To = 14,
            Duration = TimeSpan.FromMilliseconds(160),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(5),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        shakenAnimation.Completed += (s, _) =>
        {
            BoxRotateTransform.Angle = 0;
            ShowRewardCardWithAnimation(droppeditem);
        };
        BoxRotateTransform.BeginAnimation(RotateTransform.AngleProperty, shakenAnimation);
    }

    private void ShowRewardCardWithAnimation(Item item)
    {
        ShakingBoxVisual.Visibility = Visibility.Collapsed;
        RewardCardVisual.Visibility = Visibility.Visible;
        
        Brush rarityBrush = GetRarityBrush(item.ItemRarity);
        RewardCardVisual.BorderBrush = rarityBrush;
        RewardRarityText.Foreground = rarityBrush;
        RewardRarityText.Text = item.ItemRarity == Rarity.Legendary ? $"★ {item.ItemRarity.ToString().ToUpper()} ★" : item.ItemRarity.ToString().ToUpper();
        
        RewardNameText.Foreground = rarityBrush;
        RewardNameText.Text = $"\"{item.Name}\"";
        RewardDescText.Text = $"\"{item.Description}\"";
        RewardStatsSummary.Text = $" Atak: {item.AttackValue}   |    Obrona: {item.DefenseValue}   |    Życie: {item.HealthValue}";

        var scaleAnim = new DoubleAnimation()
        {
            From = 0.35,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(650),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 }
        };
        var fadeAnim = new DoubleAnimation()
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(500)
        };
        CardScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        CardScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        RewardCardVisual.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
    }
    private void CloseRewardBtn_Click(object sender, RoutedEventArgs e)
    {
        AnimationOverlay.Visibility = Visibility.Collapsed;
    }

    private void SellItemBtn_Click(object sender, RoutedEventArgs e)
    {
        if (InventoryListBox.SelectedItem is Item item)
        {
            _controller.SellItem(item);
            UpdateCoinsDisplay();
            RefreshInventoryList();
        }
    }

    private void ResetProfileBtn_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Czy na pewno chcesz zresetować profil?\nWszystkie zebrane postacie zostaną usunięte, a stan konta wróci do 1000 monet.",
            "Potwierdzenie resetu",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _controller.Reset();
            UpdateCoinsDisplay();
            RefreshInventoryList();
        }
    }

    private void BoxesTabBtn_Click(object sender, RoutedEventArgs e) => SwitchToBoxesView();
    private void InvTabBtn_Click(object sender, RoutedEventArgs e) => SwitchToInventoryView();

    private void SwitchToBoxesView()
    {
        BoxesViewGrid.Visibility = Visibility.Visible;
        InventoryViewGrid.Visibility = Visibility.Collapsed;
        
        BoxesTabBtn.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
        BoxesTabBtn.IsEnabled = false;
        
        InvTabBtn.Background = new SolidColorBrush(Color.FromRgb(51, 65, 85));
        InvTabBtn.IsEnabled = true;
        
        BoxesListBox.Focus();
    }
    
    private void SwitchToInventoryView()
    {
        BoxesViewGrid.Visibility = Visibility.Collapsed;
        InventoryViewGrid.Visibility = Visibility.Visible;
        
        InvTabBtn.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
        InvTabBtn.IsEnabled = false;
        
        BoxesTabBtn.Background = new SolidColorBrush(Color.FromRgb(51, 65, 85));
        BoxesTabBtn.IsEnabled = true;
        
        InventoryListBox.Focus();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (AnimationOverlay.Visibility == Visibility.Visible)
        {
            if (RewardCardVisual.Visibility == Visibility.Visible &&
                (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Escape))
            {
                AnimationOverlay.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
            return;
        }

        if (e.Key == Key.I)
        {
            if (InventoryViewGrid.Visibility == Visibility.Visible)
                SwitchToInventoryView();
            else
                SwitchToInventoryView();
            e.Handled = true;
        }
        else if (e.Key == Key.D1 || e.Key == Key.Escape)
        {
            SwitchToBoxesView();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && BoxesViewGrid.Visibility == Visibility.Visible)
        {
            OpenBoxBtn_Click(this, new RoutedEventArgs());
            e.Handled = true;
        }
        else if (e.Key == Key.S && InventoryViewGrid.Visibility == Visibility.Visible)
        {
            SellItemBtn_Click(this, new RoutedEventArgs());
            e.Handled = true;
        }
        else if (e.Key == Key.R && InventoryViewGrid.Visibility == Visibility.Visible)
        {
            ResetProfileBtn_Click(this, new RoutedEventArgs());
            e.Handled = true;
        }
    }

    private Brush GetRarityBrush(Rarity rarity) => rarity switch
    {
        Rarity.Common => new SolidColorBrush(Color.FromRgb(226, 232, 240)),
        Rarity.Rare => new SolidColorBrush(Color.FromRgb(56, 189, 248)),
        Rarity.Epic => new SolidColorBrush(Color.FromRgb(217, 70, 239)),
        Rarity.Legendary => new SolidColorBrush(Color.FromRgb(250, 204, 21)),
        _ => Brushes.White
    };

}