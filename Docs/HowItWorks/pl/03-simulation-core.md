# 03 · Rdzeń symulacji

**W jednym zdaniu:** reguły gry posuwają się naprzód równymi, małymi krokami zwanymi tickami (dziesięć na sekundę czasu gry); steruje nimi jeden zegar, kalendarz liczy dni w grze, a tablica ogłoszeń pozwala modułom reagować na siebie nawzajem.

**Stan:** pętla istnieje i jest przetestowana. Nie jest jeszcze połączona ze sceną i nie działają na niej żadne reguły gry (energia, produkcja, utrzymanie); dodadzą je kolejne zadania.

## Po co nam to

Komputer nie rysuje klatek w równym tempie: jedna klatka może trwać 5 ms, następna 40 ms. Gdyby reguły takie jak „panel słoneczny wytwarza 2 jednostki energii na sekundę” były liczone raz na klatkę, szybki i wolny komputer dałyby nieco inne wyniki, a test nigdy nie mógłby dokładnie przewidzieć rezultatu.

Potrzebujemy też jednego miejsca, które odpowiada na pytanie „który jest dzień?”, bo kilka reguł działa raz dziennie (utrzymanie, czyli zużycie żywności i wody, oraz wzrost trawy), a gracz może zatrzymać grę albo ją przyspieszyć.

## Jak to działa

1. **Ticki.** Symulacja nigdy nie posuwa się „o tyle, ile trwała klatka”. Posuwa się wyłącznie o całe ticki, każdy po 0,1 s czasu gry. Każda reguła jest zapisana jako „co dzieje się w jednym ticku”.
2. **Zegar zamienia klatki na ticki.** W każdej klatce scena przekazuje symulacji, ile minęło czasu rzeczywistego. Zegar dodaje ten czas do reszty z wcześniejszych klatek i liczy, ile całych ticków się w nim mieści. Przykład: klatki 0,04 s, 0,04 s i 0,04 s dają kolejno 0, 0 i 1 tick, a 0,02 s zostaje na później.
3. **Limit czasu klatki.** Jeśli jedna klatka trwa bardzo długo (gra zawiesiła się na 3 sekundy, bo coś wczytywała), zegar liczy z niej tylko 0,25 s. Inaczej gra wykonałaby nagle 30 ticków naraz, a gracz zobaczyłby, jak wszystko przeskakuje.
4. **Pauza i prędkość.** W czasie pauzy zegar pomija czas klatek, więc nie wykonuje się żaden tick. Prędkość mnoży czas klatki, zanim zostanie on policzony: przy prędkości 2 klatka trwająca 0,1 s daje 2 ticki. Pauza i prędkość są od siebie niezależne: prędkość musi być liczbą dodatnią.
5. **Kalendarz liczy dni w tickach.** Dzień trwa 28 800 ticków (48 minut gry przy normalnej prędkości; ta długość nie jest jeszcze potwierdzona). Numer dnia to po prostu liczba ticków podzielona przez 28 800. Gdy tick przekracza granicę nowego dnia, kalendarz wywołuje `OnDayStarted` jeden raz. Jeśli naraz minęło kilka dni, wywołuje je raz dla każdego z nich, po kolei.
6. **Jeden tick, w stałej kolejności.** W każdym ticku najpierw przesuwany jest kalendarz (dzięki temu reguły dzienne działają na początku pierwszego ticka dnia), a potem wykonywany jest tick każdego systemu, zawsze w kolejności, w jakiej systemy zostały dodane.
7. **Systemy.** System to jeden fragment logiki gry, na przykład „bilans energii”. Implementuje `ISimulationSystem` i w każdym ticku dostaje `TickContext`: numer ticka, długość ticka w sekundach, dzień oraz to, jaka część dnia już minęła (0 na początku dnia, blisko 1 pod koniec; panele słoneczne wykorzystają to do odróżniania dnia od nocy).
8. **Komunikaty między modułami.** `EventBus` (szyna zdarzeń) działa jak tablica ogłoszeń. Moduł publikuje komunikat, na przykład `DomeActivated`, a każdy moduł, który zasubskrybował ten rodzaj komunikatu, zostaje wywołany. Nadawca nie wie, kto słucha, więc moduły nie muszą się do siebie odwoływać.
9. **Wczytanie zapisu.** `Simulation.Restore(tickCount)` ustawia zegar i kalendarz na zapisaną liczbę ticków. Nie wykonuje żadnych ticków i nie wywołuje zdarzeń dnia, bo te dni minęły już przed zapisaniem gry.

### Najważniejsze klasy

| Klasa | Zadanie |
| --- | --- |
| `SimulationClock` | Czas klatki → całe ticki; pauza, prędkość, limit czasu klatki; łączna liczba ticków. |
| `GameCalendar` | Liczba ticków → dzień i postęp dnia; `OnDayStarted`. |
| `EventBus` | Subskrybowanie, wypisywanie się i publikowanie komunikatów (małe struktury). |
| `ISimulationSystem` | Interfejs, który implementuje każdy fragment logiki gry: `Tick(in TickContext)`. |
| `TickContext` | Informacje przekazywane każdemu systemowi w każdym ticku. |
| `Simulation` | Jest właścicielem zegara, kalendarza, szyny zdarzeń i listy systemów. `Update` (wywoływane przez scenę raz na klatkę), `Step` (wykonuje dokładnie N ticków, na potrzeby testów), `Restore` (wczytywanie), `AddSystem`. |

Inne moduły mogą odczytywać zegar i kalendarz, włączać pauzę, zmieniać prędkość i subskrybować `OnDayStarted`. Metody, które przesuwają czas do przodu, są dostępne tylko wewnątrz tego modułu. Czas może więc płynąć wyłącznie przez `Simulation`, co gwarantuje, że ticki systemów zostaną wykonane.

## Dlaczego takie rozwiązanie

| Możliwość | Dlaczego nie (albo dlaczego tak) |
| --- | --- |
| Liczenie reguł w każdej klatce z użyciem długości klatki | Wyniki zależą od liczby klatek na sekundę; testy nie mogą przewidzieć dokładnych wartości; długa klatka oznacza jeden ogromny krok. |
| `FixedUpdate` z Unity | To także stały krok, ale należy do obiektów sceny i fizyki, nie da się go uruchomić poza trybem Play, a kolejność między obiektami trudno kontrolować. |
| Kalendarz liczony w sekundach z ułamkami | Komputer nie potrafi dokładnie zapisać ułamków takich jak 0,1, więc po wielu dodawaniach dzień mógłby zacząć się o jeden tick za wcześnie albo za późno. Całe ticki są dokładne. |
| Moduły wywołujące się bezpośrednio | Każdy moduł musiałby znać pozostałe; zabraniają tego nasze reguły assembly, a zmiany rozchodziłyby się po całym kodzie. |
| Zdarzenia C# w globalnym obiekcie statycznym | Stan globalny pozostaje między kolejnymi uruchomieniami gry w edytorze i trudno go testować. Szyna zdarzeń to zwykły obiekt, którego właścicielem jest symulacja. |
| **Stałe ticki, kalendarz w tickach, szyna zdarzeń (wybrane)** | Ten sam wynik na każdym komputerze, dokładne granice dni, reguły testowalne przez `Step(n)`, niezależne moduły. |

## Gdzie to jest w projekcie

- `Assets/_Project/Scripts/Core/`: `SimulationClock`, `GameCalendar`, `EventBus`, `ISimulationSystem`, `TickContext`, `Simulation`.
- `Assets/_Project/Tests/EditMode/Core/`: testy.
- Ten moduł w ogóle nie używa kodu Unity (zabrania tego jego definicja assembly) i właśnie dzięki temu działa w testach bez sceny.

## Skąd wiemy, że działa

Testy Edit Mode (**Window > General > Test Runner > EditMode**), 83 dla tego modułu:

- `SimulationClockTests`: klatka krótsza niż tick niczego nie uruchamia, a jej czas przechodzi dalej; długa klatka jest ograniczana do 0,25 s; prędkość podwaja albo zmniejsza o połowę liczbę ticków; w czasie pauzy nic się nie wykonuje, a niepełny tick sprzed pauzy zostaje zachowany; wczytanie zapisu kasuje niepełny tick; nieprawidłowe wartości są odrzucane.
- `GameCalendarTests`: ostatni tick dnia należy jeszcze do tego dnia, a następny zaczyna nowy; przekroczenie trzech dni naraz ogłasza dni 1, 2, 3 po kolei; wczytanie zapisu niczego nie ogłasza; kalendarz nie pozwala cofnąć czasu.
- `EventBusTests`: komunikat dociera do każdego subskrybenta swojego rodzaju, w kolejności subskrypcji, i do nikogo więcej; wypisanie się działa, nawet z wnętrza metody obsługującej komunikat.
- `SimulationTests`: ticki systemów wykonują się w kolejności ich dodania; dzień jest ogłaszany przed tickami systemów; ten sam łączny czas w 4 długich klatkach albo w 40 krótkich daje te same 10 ticków; wczytanie zapisu ustawia tick i dzień, niczego nie uruchamiając.

Sprawdzenie ręczne: uruchom testy w Test Runnerze. W scenie nie ma nic do zobaczenia, dopóki symulacja nie zostanie z nią połączona.

## Pytania, które mogą paść

**Czym jest tick i dlaczego trwa 0,1 s?** Tick to jeden krok reguł gry. Dziesięć ticków na sekundę wystarcza, żeby surowce i energia zmieniały się dla gracza płynnie, a zarazem jest ich na tyle mało, że obliczenia prawie nic nie kosztują. Ta wartość to jedna stała i można ją zmienić.

**Czy na wolnym komputerze gra działa wolniej?** Nie. Rysowanych jest mniej klatek, ale każda obejmuje wtedy więcej czasu i wykonuje więcej ticków. Dopiero gdy klatka trwa dłużej niż 0,25 s, nadmiar czasu jest pomijany, żeby uniknąć nagłego przeskoku.

**Jak działa prędkość gry?** Czas każdej klatki jest mnożony przez prędkość, zanim zostanie zamieniony na ticki. Przy prędkości 2 w ciągu sekundy czasu rzeczywistego wykonuje się dwa razy więcej ticków. Każdy tick nadal obejmuje 0,1 s czasu gry, więc reguły zachowują się identycznie.

**Skąd wiadomo, który jest dzień?** To liczba ticków podzielona przez liczbę ticków w dniu (28 800). Reszta z dzielenia mówi, jaka część dnia już minęła.

**Dlaczego dni liczymy w tickach, a nie w sekundach?** Liczby całkowite są dokładne. Ułamek dziesiętny taki jak 0,1 jest zapisywany tylko w przybliżeniu, więc dodawany tysiące razy rozjeżdża się o drobne wartości. To wystarczy, żeby granica dnia wypadła w złym ticku.

**Co dzieje się ze zdarzeniami dnia po wczytaniu zapisu?** Żadne nie jest wywoływane. Wczytana liczba ticków zawiera już te dni; ponowne wywołanie zdarzeń naliczyłoby utrzymanie dwa razy.

**Jak produkcja może zareagować na aktywację kopuły, nie znając kodu kopuły?** Moduł kopuły publikuje na szynie zdarzeń komunikat `DomeActivated`, a każdy moduł, który go zasubskrybował, zostaje wywołany. Żadna ze stron nie odwołuje się do drugiej.

**W jakiej kolejności wszystko dzieje się w jednym ticku?** Najpierw kalendarz (a razem z nim reguły dzienne, w pierwszym ticku dnia), potem każdy system w kolejności, w jakiej został dodany do symulacji. Kolejność nigdy się nie zmienia, więc wyniki są powtarzalne.

**Po co jest `Step`, skoro gra używa `Update`?** `Step(n)` wykonuje dokładnie n ticków niezależnie od czasu rzeczywistego, pauzy i prędkości. Testy używają go, żeby stwierdzić: „po 100 tickach w magazynie musi być dokładnie tyle”.
