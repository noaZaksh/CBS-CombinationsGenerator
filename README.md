# מחולל הקומבינציות

אפליקציית WEB מסוג Client/Server שמחוללת את כל הסידורים האפשריים של המספרים 1..n בשורה (1 ≤ n ≤ 20).

- **צד שרת:** ‎.NET 8 (ASP.NET Core Web API)
- **צד לקוח:** Angular 20

כל החישובים מתבצעים בצד השרת. הלקוח רק מציג.

---

## הרצה

### דרישות מוקדמות
- ‎.NET 8 SDK
- Node.js 20 ומעלה ו-npm

### צד שרת
```bash
cd server
dotnet run --project CombinationsApi
```
השרת עולה בכתובת `http://localhost:5271`.

### צד לקוח
```bash
cd client
npm install
npm start
```
הלקוח עולה בכתובת `http://localhost:4200`.

כתובת השרת מוגדרת בקובץ `client/src/app/app.settings.ts`, וכתובת הלקוח המותרת ב-CORS מוגדרת ב-`server/CombinationsApi/appsettings.json` (`ClientUrl`).

### בדיקות
```bash
cd server
dotnet test          # בדיקות האלגוריתמים והלוגיקה (xUnit)

cd client
npm test             # בדיקות Angular
```

---

## מבנה הפרויקט

```
server/
  CombinationsApi/          שכבת HTTP: Controller, Program.cs (DI, CORS), טיפול בשגיאות
  CombinationsCore/         הלוגיקה, ללא תלות ב-ASP.NET
    Algorithms/             PermutationCalculator: NextPermutation, GetByIndex, n!
    Services/               CombinationsService: הפעולות Start / GetNext / GetAll / Back
    Models/                 CombinationSession ואובייקטי התשובה
    Storage/                ISessionStore: ממשק לשמירת ה-State
  CombinationsStorage/      MemorySessionStore: מימוש של ISessionStore בזיכרון
  CombinationsTests/        בדיקות xUnit

client/src/app/
  app.settings.ts           הגדרות: כתובת השרת, גודל דף
  models/                   מבנה התשובות מהשרת
  services/
    combinations-api.service.ts     קריאות HTTP בלבד
    combinations-state.service.ts   מה שמוצג במסך (Signals) והפעולות
  components/
    start-screen/           מסך 1: הזנת n
    generator-screen/       עוטף את מסכים 2-3
      total-count/          "סה"כ האפשרויות..."
      single-view/          איפוס / הבא / כל הקומבינציות
      all-view/             רשימה + דפדוף + חזור
        combination-list/   רשימת הקומבינציות בדף
        pager/              דף ראשון / קודם / מספרי דפים + קפיצה / הבא / אחרון
```

התלויות זורמות בכיוון אחד: `Api → Core ← Storage`. ה-Core לא מכיר את ה-Api ולא את מימוש האחסון, רק את הממשק `ISessionStore`.

---

## ה-API

| API | בקשה | תשובה |
|---|---|---|
| **StartAPI** | `POST /api/combinations/start`<br>גוף: `{ "n": 3 }` | `{ sessionId, n, totalCount: "6", totalCountText: "6" }` |
| **GetNextAPI** | `POST /api/combinations/{sessionId}/next` | `{ hasCombination: true, item: { number: "1", combination: [1,2,3] } }`<br>או `{ hasCombination: false, message: "אין יותר קומבינציות" }` |
| **GetAllAPI** | `GET /api/combinations/{sessionId}/all?pageSize=10&pageNumber=1` | `{ pageNumber, pageSize, totalPages, items: [...] }` |
| **Back** | `POST /api/combinations/{sessionId}/back` | הקומבינציה האחרונה שהוצגה בדף, באותו מבנה של GetNext |

שגיאות מוחזרות כ-ProblemDetails: ‏`400` על קלט לא תקין, ו-`404` על Session שלא קיים או שפג תוקפו.

**למה מספרים גדולים מוחזרים כמחרוזות:** ‏20! = 2,432,902,008,176,640,000 גדול מ-`Number.MAX_SAFE_INTEGER` של JavaScript (9,007,199,254,740,991). אם הוא היה מוחזר כמספר, הדפדפן היה מעגל אותו ומציג ערך שגוי. לכן הסה"כ, המספרים הסידוריים ומספרי הדפים עוברים כמחרוזות, והלקוח משתמש ב-`bigint` לניווט בין הדפים.

---

## החלטות במקומות שהמטלה לא מגדירה עד הסוף

1. **ל-GetAllAPI יש גם פרמטר של מספר דף.** בלעדיו אי אפשר לממש "דף קודם", "קפיצה לדף" ו"דף אחרון". מספר הדף תמיד יחסי לקומבינציה הבאה: אם מוצגת עכשיו קומבינציה 1000, דף 1 מתחיל ב-1001.
2. **API נוסף, `Back`.** המטלה דורשת ש"חזור" יציג את הקומבינציה האחרונה שהוצגה בדף, ושלחיצה על "הבא" תמשיך ממנה. כדי ש-GetNext "יתחשב בקריאה האחרונה", השרת צריך לדעת שהמשתמש חזר. ה-API הזה גם בונה בשרת את הקומבינציה שתוצג, כך שהלקוח לא מחשב כלום.
3. **"הקומבינציה האחרונה שהוצגה בדף"** היא האחרונה ברשימה של הדף שהיה על המסך ברגע הלחיצה על "חזור".
4. **גודל הדף קבוע בהגדרות הלקוח** (10) ונשלח בכל קריאה ל-GetAllAPI. אין כפתור לשינוי שלו, כי המטלה מתירה רק את כפתורי הדפדוף.

---

## תשובות לשאלות הכלליות

### 1) מהו מודל ניהול ה-State שבחרתי ולמה?

**בצד השרת: State לפי Session.** ‏`Start` יוצר Session עם מזהה ייחודי (GUID) ומחזיר אותו ללקוח. הלקוח שולח את המזהה בכל קריאה. בכל Session נשמרים רק כמה ערכים:

| שדה | משמעות |
|---|---|
| `N` | ה-n שהוזן |
| `TotalCount` | n! |
| `CurrentNumber` | המספר הסידורי של הקומבינציה שמוצגת עכשיו |
| `CurrentCombination` | הקומבינציה עצמה, n מספרים |
| `LastShownNumber` | הקומבינציה האחרונה שהוצגה בדף, בשביל "חזור" |

**למה:**
- המטלה דורשת ש-GetNext ו-GetAll "יתחשבו ב-n מ-StartAPI ובקריאה האחרונה", ושכל החישובים יהיו בשרת. לכן השרת חייב לזכור איפה כל משתמש נמצא.
- הזיכרון קטן וקבוע, כמה עשרות בתים למשתמש, ולא תלוי במספר הקומבינציות. אין שום רשימה שמחושבת מראש.
- האחסון מאחורי ממשק (`ISessionStore`), כך שאפשר להחליף זיכרון ב-Redis בלי לגעת בלוגיקה.
- Session שלא נוגעים בו 30 דקות נמחק אוטומטית (Sliding Expiration), כך ש-Sessions נטושים לא מצטברים.

**בצד הלקוח: Angular Signals בתוך שירות אחד (`CombinationsStateService`).** הקומפוננטות קוראות את ה-Signals וקוראות למתודות של השירות, ולא לשרת ישירות. בחרתי ב-Signals כי זה הכלי המובנה ב-Angular 20. הוא פשוט וקריא, ומספיק להיקף הזה. NgRx היה מוסיף הרבה קוד (Actions, Reducers, Effects) בלי תועלת אמיתית כאן. אם המערכת תגדל, אפשר לפצל את השירות או לעבור ל-NgRx Signal Store בלי לשנות את הקומפוננטות.

### 2) מה ההבדל בין NextPermutation לבין Index-Based Permutation, ומתי השתמשתי בכל אחד?

| | NextPermutation | Index-Based (Unranking) |
|---|---|---|
| **מה הוא עושה** | מקבל קומבינציה ומחזיר את הבאה אחריה בסדר לקסיקוגרפי | מקבל מספר k ומחזיר ישירות את הקומבינציה ה-k |
| **איך** | מימין מוצאים את ה-pivot, המקום הראשון שבו `a[i] < a[i+1]`. מחליפים אותו באיבר הקטן ביותר מימינו שגדול ממנו, והופכים את הזנב | שיטת הספירה העצרתית (Factorial Number System): כל ספרה ראשונה "מחזיקה" בלוק של ‎(n-1)!‎ קומבינציות, ולכן `k / (n-1)!` קובע איזה מספר יבוא ראשון, וכך הלאה |
| **סיבוכיות** | ‏O(n) לצעד, O(1) זיכרון נוסף | ‏O(n²) לקומבינציה, שהם 400 פעולות ב-n=20 |
| **צריך State?** | כן, את הקומבינציה הקודמת | לא, רק את המספר |
| **קפיצה למקום רחוק** | לא אפשרית בלי לעבור על כל הצעדים | מיידית |

**מתי השתמשתי בכל אחד:**
- **GetNext → NextPermutation.** המטלה מבקשת "לחשב רק את הקומבינציה הבאה בסדר", ויש לנו את הקומבינציה הנוכחית ב-Session. זה בדיוק התפקיד של NextPermutation, והוא הכי זול.
- **GetAll → שילוב של שניהם.** קודם Index-Based, כדי לקפוץ ישר לקומבינציה הראשונה בדף המבוקש, גם אם זה הדף האחרון של 20!. אחר כך NextPermutation עד סוף הדף, כי בתוך הדף הצעדים עוקבים, ו-O(n) לצעד זול יותר מ-O(n²) לכל קומבינציה.
- **Back → Index-Based.** בונה את הקומבינציה האחרונה שהוצגה בדף מתוך המספר שלה, בלי לשמור עותק שלה.

### 3) מה יקרה אם 3 משתמשים שונים ייכנסו בו-זמנית?

כל אחד מהם יעבוד באופן עצמאי לגמרי, בלי השפעה על האחרים:
- כל קריאה ל-`Start` יוצרת Session חדש עם GUID משלו, וכל קריאה אחרת עובדת רק על ה-Session שלה. משתמש 1 יכול להיות בקומבינציה 500 של n=10, ומשתמש 2 בקומבינציה 3 של n=4, בלי קשר ביניהם.
- `CombinationsService` הוא Singleton ללא State משלו. ה-State נמצא רק ב-Session.
- `IMemoryCache` בטוח לשימוש מכמה Threads.
- בתוך כל Session יש `lock`, למקרה שאותו משתמש שולח שתי בקשות כמעט באותו רגע (למשל לחיצה כפולה מהירה על "הבא"). כך המספרים לא מדלגים ולא מתנגשים. ה-`lock` הוא לפי Session, ולכן משתמשים שונים לא מחכים זה לזה.
- כל בקשה מחשבת מעט מאוד: O(n) ל-GetNext, ו-O(n² + pageSize·n) ל-GetAll. אין עומס שמשפיע על אחרים.

**מגבלה:** ה-State שמור בזיכרון של תהליך השרת. אם השרת יעלה מחדש, ה-Sessions יימחקו. אם ירוצו כמה מופעים של השרת מאחורי Load Balancer, כל מופע יכיר רק את ה-Sessions שלו. זו בדיוק הסיבה לעבור ל-Redis (שאלה 4).

### 4) אם נרצה להוסיף שמירה ל-Redis, אילו 2 שינויים אעשה בצד השרת?

1. **מימוש חדש של `ISessionStore`: מחלקה `RedisSessionStore`**, בפרויקט `CombinationsStorage`. המחלקה ממירה את `CombinationSession` ל-JSON ושומרת אותו ב-Redis (דרך `IDistributedCache` או `StackExchange.Redis`). המפתח הוא `combinations-session:{id}`, עם תפוגה של 30 דקות. `Get` קורא ומפענח, ו-`Save` כותב. מכיוון שה-Service כבר קורא ל-`Save` אחרי כל שינוי, הוא עובד גם מול Redis בלי שינוי.
2. **רישום ב-`Program.cs` (DI)**: מחליפים את
   ```csharp
   builder.Services.AddSingleton<ISessionStore, MemorySessionStore>();
   ```
   בהגדרת החיבור ל-Redis (`AddStackExchangeRedisCache` עם Connection String מ-`appsettings.json`) וברישום `RedisSessionStore`.

ה-Service, ה-Controller, האלגוריתמים והלקוח לא משתנים. בזכות זה האחסון נמצא מאחורי ממשק.

**הערה:** כשיש כמה מופעי שרת, ה-`lock` בזיכרון כבר לא מגן על לחיצה כפולה שמגיעה לשני מופעים שונים. אם זה יידרש, אפשר להוסיף נעילה ב-Redis או Optimistic Concurrency (מספר גרסה ב-Session).

### 5) מה החיסרון העיקרי של NextPermutation עבור n = 20?

**הוא סדרתי: כדי להגיע לקומבינציה ה-k צריך לעבור k צעדים.** ב-n=20 יש 2,432,902,008,176,640,000 קומבינציות. כדי להגיע לדף האחרון רק עם NextPermutation, גם בקצב של מיליארד צעדים בשנייה, צריך כ-77 שנים.

בנוסף, NextPermutation צריך את הקומבינציה הקודמת, ולכן אי אפשר לחשב בעזרתו דף כלשהו "מאפס".

לכן במערכת הזו NextPermutation משמש רק לצעד הבא ולתוך הדף. כל קפיצה, לדף מסוים, לדף האחרון או חזרה לקומבינציה האחרונה, נעשית ב-Index-Based ב-O(n²).
