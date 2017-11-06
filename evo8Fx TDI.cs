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

        private string upArrow = "\u25B2";
        private string downArrow = "\u25BC";
        private string diamond = "\u2666";
        private string bullet = "\u25CF";
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
        private const VerticalAlignment vAlign = VerticalAlignment.Top;
        private const HorizontalAlignment hAlign = HorizontalAlignment.Center;

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
            ChartObjects.DrawHorizontalLine("37", 37, Colors.PaleGreen, 1, LineStyle.Lines);
            ChartObjects.DrawHorizontalLine("32", 32, Colors.LimeGreen, 1, LineStyle.Lines);

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

            //write arrows
            var volume = MarketSeries.TickVolume[index];
            var volume1 = MarketSeries.TickVolume[index - 1];
            double volume2 = MarketSeries.TickVolume[index - 2];
            var high = MarketSeries.High[index];
            var low = MarketSeries.Low[index];
            var close = MarketSeries.Close[index];
            double close1 = MarketSeries.Close[index - 1];
            double close2 = MarketSeries.Close[index - 2];
            var currentHighMinusLow = high - low;
            var previousHighMinusLow = MarketSeries.High[index - 1] - MarketSeries.Low[index - 1];

            arrowName = string.Format("bulletSell {0}", index);
            y = high + arrowOffset;
            ChartObjects.DrawText(arrowName, bullet, x, y, vAlign, hAlign, Colors.Orange);

        }


    }
}
