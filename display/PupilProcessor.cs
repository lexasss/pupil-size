using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;

namespace PupilSizeDisplay;

public enum Source { Diameter, Area }

public class PupilProcessor
{
    public TextBlock Value => _value;
    public PupilProcessor(LiveData graph, Border bar, TextBlock value, Border max)
    {
        _graph = graph;
        _bar = bar;
        _value = value;
        _max = max;
    }

    public void Clear()
    {
        _maxPupilSize = 0;
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

        _value.Text = size.ToString("F2");
        _queue.Enqueue(pupil);

        if (_queue.Count == MAX_QUEUE_SIZE)
        {
            Update(source);
        }
    }

    // Internal

    private const int MAX_QUEUE_SIZE = 5;       // samples to average
    private const double DATA_SOURCE_FREQUENCY = 120; // Hz
    private const double DATA_UPDATE_FREQUENCY = DATA_SOURCE_FREQUENCY / MAX_QUEUE_SIZE;  // Hz

    private readonly LiveData _graph;
    private readonly Border _bar;
    private readonly TextBlock _value;
    private readonly Border _max;
    private readonly Queue<Types.Pupil> _queue = new();

    private double _maxPupilSize = 0;

    private void Update(Source source)
    {
        var mean = _queue.Average(pupil => source switch
        {
            Source.Diameter => pupil.Diameter3d,
            Source.Area => pupil.Diameter,
            _ => 0
        });

        if (_maxPupilSize < mean)
            _maxPupilSize = mean;

        SetValue(mean, _maxPupilSize);

        _queue.Clear();
    }

    private void SetValue(double mean, double max)
    {
        _graph.Add(Utils.Timestamp.Sec, mean);

        var barHeight = (_bar.Parent as Grid)!.ActualHeight * mean / max;
        _bar.Height = barHeight;

        var bottomMax = Math.Max(_max.Margin.Bottom - 1, barHeight);
        _max.Margin = new Thickness(0, 0, 0, bottomMax);
    }
}
