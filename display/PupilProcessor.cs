using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace PupilSizeDisplay;

public enum Source { Diameter, Area }

public class PupilProcessor : INotifyPropertyChanged
{
    public string MeanString { get; private set; } = "0";

    public event PropertyChangedEventHandler? PropertyChanged;

    public PupilProcessor(LiveData graph, Border bar, Border level)
    {
        _graph = graph;
        _bar = bar;
        _level = level;
    }

    public void Clear()
    {
        _maxPupilSize = 0;
        _means.Clear();
        _slidingMeans.Clear();
        _graph.Reset(DATA_UPDATE_FREQUENCY / DATA_SOURCE_FREQUENCY / LiveData.PixelsPerPoint, 0);
    }

    public void Add(Types.Pupil pupil, Source source)
    {
        var size = source switch
        {
            Source.Diameter => pupil.Diameter3d,
            Source.Area => pupil.Diameter,
            _ => 0
        };

        if (size < 0)
            return;

        MeanString = size.ToString("F2");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeanString)));

        _means.Enqueue(pupil);

        if (_means.Count == MAX_QUEUE_SIZE)
        {
            Update(source);
        }
    }

    // Internal

    private const int MAX_QUEUE_SIZE = 5;       // samples to average
    private const int SLIDING_QUEUE_SIZE = 50;  // means to collect
    private const double DATA_SOURCE_FREQUENCY = 120; // Hz
    private const double DATA_UPDATE_FREQUENCY = DATA_SOURCE_FREQUENCY / MAX_QUEUE_SIZE;  // Hz

    private readonly LiveData _graph;
    private readonly Border _bar;
    private readonly Border _level;
    private readonly Queue<Types.Pupil> _means = new();
    private readonly Queue<double> _slidingMeans = new();

    private double _maxPupilSize = 0;

    private void Update(Source source)
    {
        var mean = _means.Average(pupil => source switch
        {
            Source.Diameter => pupil.Diameter3d,
            Source.Area => pupil.Diameter,
            _ => 0
        });
        _means.Clear();

        if (_maxPupilSize < mean)
            _maxPupilSize = mean;

        UpdateMean(mean);

        _slidingMeans.Enqueue(mean);
        if (_slidingMeans.Count > SLIDING_QUEUE_SIZE)
            _slidingMeans.Dequeue();

        int i = 0;
        double halfSlidingQueueSize = SLIDING_QUEUE_SIZE / 2;
        var oldMean = _slidingMeans.Sum(size => (i++ < halfSlidingQueueSize) ? size : 0) / halfSlidingQueueSize;

        UpdateSlidingMean(oldMean);
    }

    private void UpdateMean(double mean)
    {
        _graph.Add(Utils.Timestamp.Sec, mean);

        var height = (_bar.Parent as Grid)!.ActualHeight * mean / _maxPupilSize;
        _bar.Height = height;
    }

    private void UpdateSlidingMean(double mean)
    {
        var bottomMargin = (_bar.Parent as Grid)!.ActualHeight * mean / _maxPupilSize;
        _level.Margin = new Thickness(0, 0, 0, bottomMargin);
    }

    private void UpdateMax(double mean)
    {
        var bottomMargin = (_bar.Parent as Grid)!.ActualHeight * mean / _maxPupilSize;
        bottomMargin = Math.Max(_level.Margin.Bottom - 1, bottomMargin);
        _level.Margin = new Thickness(0, 0, 0, bottomMargin);
    }
}
