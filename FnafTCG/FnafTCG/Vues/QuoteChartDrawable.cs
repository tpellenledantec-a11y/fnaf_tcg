using FnafTCG.Modeles;
using Microsoft.Maui.Graphics;

namespace FnafTCG.Vues;

public class QuoteChartDrawable : IDrawable
{
    public IReadOnlyList<Quote> Quotes { get; set; } = Array.Empty<Quote>();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Quotes.Count == 0 || dirtyRect.Width <= 16 || dirtyRect.Height <= 16)
            return;

        float minimum = (float)Quotes.Min(quote => quote.Price);
        float maximum = (float)Quotes.Max(quote => quote.Price);
        float amplitude = Math.Max(maximum - minimum, 0.01f);
        float largeur = dirtyRect.Width - 16;
        float hauteur = dirtyRect.Height - 16;
        using var chemin = new PathF();
        for (int i = 0; i < Quotes.Count; i++)
        {
            float x = dirtyRect.Left + 8 + (Quotes.Count == 1 ? largeur / 2
                : largeur * i / (Quotes.Count - 1));
            float y = dirtyRect.Top + 8 + hauteur *
                (1 - ((float)Quotes[i].Price - minimum) / amplitude);
            if (i == 0)
                chemin.MoveTo(x, y);
            else
                chemin.LineTo(x, y);

            if (Quotes.Count == 1)
            {
                canvas.FillColor = Color.FromArgb("#FFAE19");
                canvas.FillCircle(x, y, 3);
            }
        }
        canvas.StrokeColor = Color.FromArgb("#FFAE19");
        canvas.StrokeSize = 2;
        canvas.DrawPath(chemin);
    }
}
