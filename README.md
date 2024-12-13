Test are separeted in 2 parts :
controllers :
Function in Controller :
Single Responsibility Principle: The controller's responsibility is to handle HTTP requests and responses, not to validate or calculate business logic  business logic should be check in my service

Services :
buisness logic 



Responsibilities:
•	Manage library services including book check-out, late fee payments, and library account management. 
•	Integrate with the university library system to track book availability and user activity. 

Key Features:
1.	Book Check-Out and Return: Allow users to check out and return books using their service card. 
2.	Late Fee Payments: Process payments for overdue books directly through the service card. 	
3.	Account Management: Provide users with the ability to view their library account details, including borrowed books and outstanding fees. 

Challenges for Unit Testing:
•	Mocking interactions with the library database and tracking systems. 
•	Ensuring accurate fee calculations and payment processing. 
•	Testing edge cases like book renewals, multiple check-outs, and fee waivers.

Class :
user : Id, name, firstname, fees
Book : Id, Title, feePrice, BorrowingDay(How many days you can borrow this book), available
LibraryTransaction : Id, UserId, BookId, TransactionType(Checkout, Return) Date

ILibraryService :
    bool CheckOutBook(int userId, string bookId); : faire le checkout d’un livre
    bool ReturnBook(int userId, string bookId); : faire le retour d’un book
    decimal GetOutstandingFees(int userId); : recevoir les coûts d’amande
    List<LibraryTransaction> GetUserLibraryTransactions(int userId); : recevoir toutes les transactions d’un user
