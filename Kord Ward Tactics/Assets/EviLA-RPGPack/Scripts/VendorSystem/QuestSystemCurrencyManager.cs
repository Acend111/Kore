using UnityEngine;
using System;
using System.Collections.Generic;

namespace EviLA.AddOns.RPGPack.Currency
{
	[Serializable]
	public class QuestSystemCurrencyManager : MonoBehaviour { 

		private static QuestSystemCurrencyManager instance;

		public static QuestSystemCurrencyManager Instance{
			get{
				return instance;
			}
		}

		public void Awake(){
			if (instance == null) {
				instance = this;
				DontDestroyOnLoad (this);
			}
		}

		public List<Currency> currencies = new List<Currency>();

		public void Start( ){
			
			//vHUDController.instance.ShowText( "Selling Price LKR to USD " + CurrencyControl.Instance.Convert( currencies.Find( c => c.name.Equals("USD") ), currencies.Find( c => c.name.Equals("LKR")), 1, false ).ToString());
		}

	}

	public class CurrencyControl {

		private static readonly CurrencyControl instance = new EviLA.AddOns.RPGPack.Currency.CurrencyControl();

		CurrencyControl() {

		}

		public static CurrencyControl Instance {
			get{
				return instance;
			}
		}

		// isSellingPrice = true --> Vendor Sells and Customer buys = amount / buying rate
		// isSellingPrice = false --> Vendor Buys and Customer sells = amount * selling rate

		public float Convert(Currency a, Currency b, float amount, bool isSellingPrice = false) {
			var rate = a.GetConversionRateFor(b);
			if( isSellingPrice )
			{
				return (float) ( amount / rate.BuyingRate );
			} else {
				return (float) (amount * rate.SellingRate );
			}
		}


	}

	[Serializable]
	public class Currency {

		public string name;
		public List<Conversion> conversionRates = new List<Conversion>();

		public Conversion GetConversionRateFor(Currency b){
			var conversion = conversionRates.Find( c => c.currency.Equals(b.name));
			return conversion;
		}
	}

	[Serializable]
	public class Conversion{
		public string currency;
		public float SellingRate;
		public float BuyingRate;

	}

}
