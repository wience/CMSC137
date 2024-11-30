using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class ResourceManager : Singleton<ResourceManager>
{

#if IN_APP
    public static event Action<string> ProductPurchased;
    public static event Action<bool> ProductRestored;
#endif

    public static string NoAdsProductID => GameSettings.Default.InAppSetting.removeAdsId;
    public static bool AbleToRestore => EnableAds;

    public static bool EnableAds
    {
        get => PrefManager.GetBool(nameof(EnableAds), true);
        set => PrefManager.SetBool(nameof(EnableAds), value);
    }


     
    protected override void OnInit()
    {
        base.OnInit();
       
            }

    private void PurchaserOnRestorePurchased(bool restored)
    {
      

      
     }


    public static void RestorePurchase()
    {
        Debug.Log("Restore InAppPurchase");
        }

    private static void PurchaseInApp(string productId, Action<bool> completed = null)
    {
        
    }

   

    public static void PurchaseNoAds(Action<bool> completed)
    {
        PurchaseInApp(NoAdsProductID, (success) =>
        {
            if (success)
                EnableAds = false;

            completed?.Invoke(success);
        });
    }
}

