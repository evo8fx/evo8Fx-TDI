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
                    ChartObjects.DrawText("EMA_Dots" + index, bullet, index, 30, vAlign, hAlign, upColor);
                    ChartObjects.DrawText("MAC_Dots" + index, bullet, index, 26, vAlign, hAlign, upColor);
                }

                else if (EMA.Result[index] < EMA.Result[index - 1] && Mac.Histogram[index] < Mac.Histogram[index - 1])
                {
                    ChartObjects.DrawText("EMA_Dots" + index, bullet, index, 30, vAlign, hAlign, dnColor);
                    ChartObjects.DrawText("MAC_Dots" + index, bullet, index, 26, vAlign, hAlign, dnColor);
                }
                else
                {
                    ChartObjects.DrawText("EMA_Dots" + index, bullet, index, 30, vAlign, hAlign, Colors.DimGray);
                    ChartObjects.DrawText("MAC_Dots" + index, bullet, index, 26, vAlign, hAlign, Colors.DimGray);

                }
            }

            //write arrows

            var high = MarketSeries.High[index];


            arrowName = string.Format("bulletSell {0}", index);
            y = high + arrowOffset;
           // ChartObjects.DrawText(arrowName, bullet, x, y, vAlign, hAlign, Colors.Orange);

        }


    }
}
