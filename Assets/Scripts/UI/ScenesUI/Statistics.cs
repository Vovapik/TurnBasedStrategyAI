using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[CreateAssetMenu(menuName = "GameConfig")]

public class Statistics : MonoBehaviour
{
    public Button backToMenuButton;

    public TMP_Text statsText;        
    public TMP_Text configLeftText;   
    public TMP_Text configRightText;  

    private void Start()
    {
        RefreshStatisticsUI();
        RefreshConfigUI();

        backToMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("StartMenuScene");
        });
    }


    private void RefreshStatisticsUI()
    {
        var s = StatsManager.stats;

        statsText.text =
            $"Units Created: {s.totalUnitsCreated}\n" +
            $"Units Killed: {s.totalUnitsKilled}\n" +
            $"Gold Earned: {s.totalGoldEarned}\n" +
            $"Gold Spent: {s.totalGoldSpent}\n" +
            $"Total Turns: {s.totalTurnsPlayed}";
    }


    private void RefreshConfigUI()
{
    var cfg = GameConfigProvider.Config;
    if (cfg == null)
    {
        configLeftText.text = "Config not found";
        configRightText.text = "";
        return;
    }

    configLeftText.text =
        "<b>ECONOMY</b>\n" +
        $"Castle Income: {cfg.castleIncomePerTurn}\n" +
        $"Fortpost Income: {cfg.fortpostIncomePerTurn}\n" +
        $"Starting Gold: {cfg.startingGold}\n" +

        "<b>COSTS</b>\n" +
        $"Fortpost Cost: {cfg.costFortpost}\n" +
        $"Warrior Cost: {cfg.costWarrior}\n" +
        $"Archer Cost: {cfg.costArcher}\n" +
        $"Chivalry Cost: {cfg.costChivalry}\n" +
        $"Engineer Cost: {cfg.costEngineer}\n" +
        $"Catapult Cost: {cfg.costCatapult}\n" +

        "<b>BUILDINGS</b>\n" +
        $"Castle HP: {cfg.castleHp}\n" +
        $"Fortpost HP: {cfg.fortpostHp}\n" +

        "<b>WARRIOR</b>\n" +
        $"HP: {cfg.warriorHp}\n" +
        $"Damage: {cfg.warriorDamage}\n";


    configRightText.text =
        "<b>ARCHER</b>\n" +
        $"HP: {cfg.archerHp}\n" +
        $"Damage: {cfg.archerDamage}\n" +
        $"Range: {cfg.archerRange}\n" +

        "<b>CHIVALRY</b>\n" +
        $"HP: {cfg.chivalryHp}\n" +
        $"Damage: {cfg.chivalryDamage}\n" +
        "Can act twice per turn\n" +

        "<b>ENGINEER</b>\n" +
        $"HP: {cfg.engineerHp}\n" +
        $"Damage: {cfg.engineerDamage}\n" +

        "<b>CATAPULT</b>\n" +
        $"HP: {cfg.catapultHp}\n" +
        $"Damage vs Buildings: {cfg.catapultDamage}\n" +
        $"Damage vs Units: {cfg.catapultDamageVsUnits}\n" +
        $"Range: {cfg.catapultRange}\n" +

        "<b>MULTIPLIERS</b>\n" +
        $"NonCat → Castle: x{cfg.damageMultiplierOnCastleFromNonCatapult}\n" +
        $"NonCat → Fortpost: x{cfg.damageMultiplierOnFortpostFromNonCatapult}\n" +
        $"Catapult → Castle: x{cfg.catapultCastleMultiplier}\n" +
        $"Catapult → Fortpost: x{cfg.catapultFortMultiplier}\n";
}


    public void OnResetStatsButtonPressed()
    {
        StatsManager.Reset();
        RefreshStatisticsUI();
    }
}
