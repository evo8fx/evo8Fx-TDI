using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Levels(32, 37, 50, 63, 68)]
    [Indicator(AccessRights = AccessRights.None)]
    public class evo8FxTDI : Indicator
    {
        private RelativeStrengthIndex _rsi;
        private MovingAverage _price;
        private MovingAverage _signal;
        private BollingerBands _bollingerBands;

        private string upArrow = "▲";
        private string downArrow = "▼";
        private string diamond = "♦";
        private string bullet = "●";
        private string stop = "x";

        enum Signals
        {
            None,
            Buy,
            Sell
        }

        [Parameter()]
        public DataSeries Source { get; set; }

        [Parameter("RSI Period", DefaultValue = 13)]
        public int RsiPeriod { get; set; }

        [Parameter("Price Period", DefaultValue = 2)]
        public int PricePeriod { get; set; }

        [Parameter("Signal Period", DefaultValue = 7)]
        public int SignalPeriod { get; set; }

        [Parameter("Volatility Band", DefaultValue = 34)]
        public int Volatility { get; set; }

        [Parameter("Standard Deviations", DefaultValue = 2, Step = 0.1)]
        public double StDev { get; set; }

        [Parameter("Price Ma Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType PriceMaType { get; set; }

        [Parameter("Signal Ma Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType SignalMaType { get; set; }

        [Output("Upper Band", Color = Colors.LightSkyBlue)]
        public IndicatorDataSeries Up { get; set; }

        [Output("Lower Band", Color = Colors.LightSkyBlue)]
        public IndicatorDataSeries Down { get; set; }

        [Output("Middle Band", Color = Colors.Orange, Thickness = 2)]
        public IndicatorDataSeries Middle { get; set; }

        [Output("Price", Color = Colors.Green, Thickness = 2)]
        public IndicatorDataSeries PriceSeries { get; set; }

        [Output("Signal", Color = Colors.Red, Thickness = 2)]
        public IndicatorDataSeries SignalSeries { get; set; }

        private double arrowOffset;


        // Elders Impulse ---------------------------------------------------------------
        private VerticalAlignment vAlign = VerticalAlignment.Bottom;
        private HorizontalAlignment hAlign = HorizontalAlignment.Center;
        private ExponentialMovingAverage EMA;
        private MacdHistogram Mac;
        private Colors upColor = Colors.DarkGreen;
        private Colors dnColor = Colors.DarkRed;

        [Parameter("- Show Impulse -------", DefaultValue = true)]
        public bool enable_EldersImpulse { get; set; }

        [Parameter("Impulse EMA Period", DefaultValue = 13)]
        public int EMAPeriod { get; set; }

        [Parameter("Impulse LongCycle", DefaultValue = 26)]
        public int LongCycle { get; set; }

        [Parameter("Impulse ShrtCycle", DefaultValue = 12)]
        public int ShrtCycle { get; set; }

        [Parameter("Impulse Signal", DefaultValue = 9)]
        public int Signal { get; set; }

        private Signals EldersImpulse_Signal = Signals.None;

        protected override void Initialize()
        {

            // TDI Shit
            _rsi = Indicators.RelativeStrengthIndex(Source, RsiPeriod);
            _bollingerBands = Indicators.BollingerBands(_rsi.Result, Volatility, StDev, MovingAverageType.Simple);
            _price = Indicators.MovingAverage(_rsi.Result, PricePeriod, PriceMaType);
            _signal = Indicators.MovingAverage(_rsi.Result, SignalPeriod, SignalMaType);

            // colorize Lines
            ChartObjects.DrawHorizontalLine("68", 68, Colors.Red, 1, LineStyle.Lines);
            ChartObjects.DrawHorizontalLine("63", 63, Colors.OrangeRed, 1, LineStyle.Lines);
            ChartObjects.DrawHorizontalLine("50", 50, Colors.Orange, 1, LineStyle.Solid);
            ChartObjects.DrawHorizontalLine("37", 37, Colors.PaleGreen, 1, LineStyle.Lines);
            ChartObjects.DrawHorizontalLine("32", 32, Colors.LimeGreen, 1, LineStyle.Lines);

            // Elders Impulse  
            if (enable_EldersImpulse)
            {
                EMA = Indicators.ExponentialMovingAverage(MarketSeries.Close, EMAPeriod);
                Mac = Indicators.MacdHistogram(LongCycle, ShrtCycle, Signal);
            }
            // graphic objects
            arrowOffset = Symbol.PipSize * 5;

        }



        public override void Calculate(int index)
        {
            int x = index;
            double y;
            string arrowName;

            // output for charting
            Up[index] = _bollingerBands.Top[index];
            Down[index] = _bollingerBands.Bottom[index];
            Middle[index] = _bollingerBands.Main[index];

            PriceSeries[index] = _price.Result[index];
            SignalSeries[index] = _signal.Result[index];

            // Elders Impulse
            if (enable_EldersImpulse)
            {
                if (EMA.Result[index] > EMA.Result[index - 1] && Mac.Histogram[index] > Mac.Histogram[index - 1])
                {
                    ChartObjects.DrawText("EMA_Dots" + index, bullet, x, 26, vAlign, hAlign, upColor);
                    ChartObjects.DrawText("MAC_Dots" + index, bullet, x, 22, vAlign, hAlign, upColor);
                    EldersImpulse_Signal = Signals.Buy;
                }

                else if (EMA.Result[index] < EMA.Result[index - 1] && Mac.Histogram[index] < Mac.Histogram[index - 1])
                {
                    ChartObjects.DrawText("EMA_Dots" + index, bullet, x, 26, vAlign, hAlign, dnColor);
                    ChartObjects.DrawText("MAC_Dots" + index, bullet, x, 22, vAlign, hAlign, dnColor);
                    EldersImpulse_Signal = Signals.Sell;
                }
                else
                {
                    ChartObjects.DrawText("EMA_Dots" + index, bullet, x, 26, vAlign, hAlign, Colors.DimGray);
                    ChartObjects.DrawText("MAC_Dots" + index, bullet, x, 22, vAlign, hAlign, Colors.DimGray);
                    EldersImpulse_Signal = Signals.None;
                }
            }

            //write arrows

            var high = MarketSeries.High[index];


            
            //x = _bollingerBands.Main[index];
            
            // ChartObjects.DrawText(arrowName, bullet, x, y, vAlign, hAlign, Colors.Orange);

            //--------------------------------------------------------------------------------
            // TDI Sell signal      ----------------------------------------------------------
            //--------------------------------------------------------------------------------
            // Price / MAIN cross down (above center)
            if (Middle[index] > 50 & _price.Result.HasCrossedBelow(_bollingerBands.Main, 0)){
                arrowName = string.Format("TDI-sell-signal {0}", index);
                y = Middle[index] + 6;
                ChartObjects.DrawText(arrowName, downArrow, x, y, vAlign, hAlign, Colors.Red);
            }
            // Price / Signal cross down (above 63)
            if (SignalSeries[index] >= 63 & _price.Result.HasCrossedBelow(_signal.Result, 0)){                
                arrowName = string.Format("TDI-sell-signal2 {0}", index);
                y = SignalSeries[index] + 4;
                ChartObjects.DrawText(arrowName, stop, x, y, vAlign, hAlign, Colors.Gold);
            }
            // Signal / Main Cross down (above 63)
            if (Middle[index] > 63 & _signal.Result.HasCrossedBelow(_bollingerBands.Main, 0)){
                arrowName = string.Format("TDI-sell-signal3 {0}", index);
                y = Middle[index] + 6;
                ChartObjects.DrawText(arrowName, downArrow, x, y, vAlign, hAlign, Colors.IndianRed);
            }
            // High Price / Main cross down (above 63)
            if (SignalSeries[index] >= 63 & _price.Result.HasCrossedBelow(_bollingerBands.Main, 0)){
                arrowName = string.Format("TDI-sell-signal4 {0}", index);
                y = Middle[index] + 8;
                ChartObjects.DrawText(arrowName, downArrow, x, y, vAlign, hAlign, Colors.Red);
            }
            //--------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------
            // TDI Buy signal      -----------------------------------------------------------
            //--------------------------------------------------------------------------------
            // Price / MAIN cross up (below center)
            if (Middle[index] < 50 & _price.Result.HasCrossedAbove(_bollingerBands.Main, 0))
            {
                arrowName = string.Format("TDI-buy-signal {0}", index);
                y = Middle[index] - 0;
                ChartObjects.DrawText(arrowName, upArrow, x, y, vAlign, hAlign, Colors.Green);
            }
            //--------------------------------------------------------------------------------
            // Price / Signal cross up (below 37)
            if (SignalSeries[index] <= 37 & _price.Result.HasCrossedAbove(_signal.Result, 0))
            {
                arrowName = string.Format("TDI-buy-signal2 {0}", index);
                y = SignalSeries[index] - 2;
                ChartObjects.DrawText(arrowName, stop, x, y, vAlign, hAlign, Colors.Gold);
            }
            // Signal / Main Cross up (below 37)
            if (Middle[index] < 37 & _signal.Result.HasCrossedAbove(_bollingerBands.Main, 0))
            {
                arrowName = string.Format("TDI-buy-signal3 {0}", index);
                y = Middle[index] - 0;
                ChartObjects.DrawText(arrowName, upArrow, x, y, vAlign, hAlign, Colors.LightGreen);
            }
            // Low Price / Main cross up (below 37)
            if (Middle[index] <= 37 & _price.Result.HasCrossedAbove(_bollingerBands.Main, 0))
            {
                arrowName = string.Format("TDI-buy-signal4 {0}", index);
                y = Middle[index] - 2;
                ChartObjects.DrawText(arrowName, upArrow, x, y, vAlign, hAlign, Colors.Green);
            }

            // 
            arrowName = string.Format("RSI-Main_direction {0}", index);
            if (Middle[index] > PriceSeries[index])
            {
                y = Middle[index] + 6;
                ChartObjects.DrawText(arrowName, ".", x, y, vAlign, hAlign, Colors.Red);
            }
            else if (Middle[index] < PriceSeries[index])
            {
                y = Middle[index] + 3;
                ChartObjects.DrawText(arrowName, ".", x, y, vAlign, hAlign, Colors.Green);
            }
            



        }


    }
}
