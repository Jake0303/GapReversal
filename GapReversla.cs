
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class GapReversal : Strategy
	{
		private bool stopTrading = false;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"This strategy will look to fill gaps.";
				Name										= "GapReversal";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				minGap											= 10;
				Stop										= 40;
				IsTradeLong									= true;
				IsTradeShort								= false;
				minVol											= 20000;
				draw = true;
			}
			else if (State == State.Configure)
			{
				AddChartIndicator(VOL());
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;
			//Reset our trading every new day
			if (Bars.BarsSinceNewTradingDay == 0) {
				stopTrading = false;
				if(Math.Abs(PriorDayOHLC().PriorClose[0] - Close[0]) < minGap)
					stopTrading = true;
			}
			
			// Reset Trail Stop
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				SetTrailStop(CalculationMode.Ticks, Stop);
			}
			
			//Make sure we are not in a position
			if (Position.MarketPosition == MarketPosition.Flat && stopTrading == false
				&& Volume[0] > minVol) {
				//Check if gap up
				if (PriorDayOHLC().PriorClose[0] < Close[0] && IsTradeShort) {
					EnterShort(100);
					//Set profit target at 1%
					//SetProfitTarget(CalculationMode.Percent, .01d);
					//Set trail stop at 1%
					//SetStopLoss(CalculationMode.Currency, 50);
					stopTrading = true;
				}
				//Check if gap down
				if (PriorDayOHLC().PriorClose[0] > Close[0] && IsTradeLong) {
					EnterLong(100);
					//Set profit target at 1%
					//SetProfitTarget(CalculationMode.Percent, .01d);
					//Set trail stop at 1%
					//SetStopLoss(CalculationMode.Currency, 50);
					stopTrading = true;
				}
			}
				if (draw)
					Draw.Line(this, "Gap"+CurrentBar, false, Bars.BarsSinceNewTradingDay, PriorDayOHLC().PriorClose[0] , 0, PriorDayOHLC().PriorClose[0], Brushes.Green, DashStyleHelper.Solid, 3);
		}
		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Min Gap", Description="Percent of Gap", Order=1, GroupName="NinjaScriptStrategyParameters")]
		public double minGap
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Stop", Description="Number of ticks for Trailing Stop Loss", Order=2, GroupName="NinjaScriptStrategyParameters")]
		public int Stop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Trade Long", Description="Trade Long", Order=3, GroupName="NinjaScriptStrategyParameters")]
		public bool IsTradeLong
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Trade Short", Description="Trade Short", Order=4, GroupName="NinjaScriptStrategyParameters")]
		public bool IsTradeShort
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Draw Gaps", Description="Draw Gaps", Order=4, GroupName="NinjaScriptStrategyParameters")]
		public bool draw
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Min Volume", Description="Minimum Volume for signal", Order=5, GroupName="NinjaScriptStrategyParameters")]
		public int minVol
		{ get; set; }
	}
	
}
