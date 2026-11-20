using GachaSimulator.Data;
using GachaSimulator.Models;

namespace GachaSimulatorGUI.Controllers;

public class GameController
{
    private readonly DatabaseService _db;
    
    public Player CurrentPlayer { get; private set; }
    public List<Blindbox> Boxes { get; private set; }

    public GameController()
    {
        _db = new DatabaseService();
        _db.InitializeDatabase();
        
        CurrentPlayer = _db.LoadPlayer();
        Boxes = _db.LoadBlindBoxes();
    }

    public Item? OpenBox(Blindbox box)
    {
        if (!CurrentPlayer.CanAfford(box.Cost))
        {
            return null;
        }
        
        CurrentPlayer.DeductCoins(box.Cost);
        _db.UpdatePlayerCoins(CurrentPlayer.Coins);

        Item droppedItem = box.Open();
        CurrentPlayer.AddItemToInventory(droppedItem);
        _db.AddItemToInventory(droppedItem);
        
        return droppedItem;
    }

    public void SellItem(Item item)
    {
        CurrentPlayer.SellItems(item);
        _db.RemoveItemFromInventory(item.Id);
        _db.UpdatePlayerCoins(CurrentPlayer.Coins);
    }

    public void Reset()
    {
        CurrentPlayer.Reset();
        _db.ResetPlayer();
    }
}