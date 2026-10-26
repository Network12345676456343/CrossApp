namespace Core.Events;

public enum LoanChangeKind
{
    Issued,     // Примірник видано читачеві
    Returned,   // Примірник повернуто до бібліотеки
    Cancelled   // Видачу скасовано/анульовано
}