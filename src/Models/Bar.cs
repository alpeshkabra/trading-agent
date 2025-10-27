namespace Quant.Models;

public readonly record struct Bar(
    DateOnly Date, double Open, double High, double Low, double Close, double Volume);
