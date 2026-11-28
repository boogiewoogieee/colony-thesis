# Jak to działa

Opisy wszystkich systemów, narzędzi i technik użytych w projekcie, napisane prostym językiem. Dzięki nim każda osoba z zespołu potrafi wyjaśnić projekt na obronie pracy, a nie tylko ta, która zbudowała daną część.

To jest wersja główna. Wersja angielska: [How it works](../en/README.md). Mówi to samo, dokument po dokumencie.

Każdy dokument odpowiada na te same pytania, w tej samej kolejności:

1. **W jednym zdaniu:** czym to jest.
2. **Po co nam to:** jaki problem rozwiązuje w naszej grze.
3. **Jak to działa:** krok po kroku, najpierw bez kodu, potem najważniejsze klasy.
4. **Dlaczego takie rozwiązanie:** jakie inne możliwości rozważaliśmy i dlaczego ich nie wybraliśmy.
5. **Gdzie to jest w projekcie:** pliki i klasy.
6. **Skąd wiemy, że działa:** testy i sposób sprawdzenia ręcznie.
7. **Pytania, które mogą paść:** pytania, które może zadać recenzent, z krótkimi odpowiedziami.

Nazwy klas, plików, folderów i pozycji menu Unity zostają po angielsku, bo tak wyglądają w kodzie, w Unity i na GitHubie.

## Spis treści

| Nr | Temat | Zakres |
| --- | --- | --- |
| 01 | [Repozytorium i praca zespołowa](01-repository-and-teamwork.md) | Git, Git LFS, gałęzie, pull requesty, chroniona gałąź `main`, pliki meta, łączenie scen |
| 02 | [Architektura kodu](02-code-architecture.md) | Symulacja oddzielona od warstwy prezentacji, assembly, dane w ScriptableObjectach |
| 03 | [Rdzeń symulacji](03-simulation-core.md) | Stałe ticki, zegar z pauzą i prędkością, kalendarz i dni, komunikaty między modułami |
| 05 | [Siatka i stawianie budynków](05-grid-and-building-placement.md) | Ukryta siatka kwadratowa, obrysy budynków, przyciąganie do siatki, sprawdzanie nachodzenia, jeden poziom zabudowy dla wszystkich budynków |

Numer 04 (pakiety i scena główna) jest zarezerwowany dla dokumentu, który powstanie razem z tym zadaniem.

Kolejne dokumenty powstają razem z kolejnymi systemami (pakiety i scena główna, magazyn, energia, produkcja, koloniści, zapis gry, pogoda, kopuła, trawa, najazdy, ekspedycje, samouczek).

## Słowniczek

| Pojęcie | Znaczenie |
| --- | --- |
| Assembly | Osobno kompilowana część kodu. Każdy nasz moduł to jedno assembly. |
| Commit | Zapisana w gicie migawka zmian razem z opisem, co zmieniono. |
| Dzień w grze | Stała liczba ticków (28 800, czyli 48 minut gry przy normalnej prędkości; wartość do potwierdzenia). Reguły dzienne uruchamiają się, gdy zaczyna się nowy dzień. |
| Klatka | Jeden obraz narysowany na ekranie. Klatki trwają różnie długo, ticki zawsze tyle samo. |
| Komórka siatki | Jeden kwadrat niewidocznej siatki budowania (10 m × 10 m). |
| LFS | Git Large File Storage: przechowuje duże pliki binarne (modele, tekstury, dźwięk) poza zwykłą historią gita. |
| Model zastępczy | Prosta bryła (zwykłe pudełko) używana, dopóki nie ma właściwego modelu. W kodzie: placeholder. |
| Obrys budynku | Prostokąt komórek siatki, który zajmuje budynek, na przykład 2 × 2. W kodzie: footprint. |
| Poziom zabudowy | Jedna wysokość, na której stoją wszystkie budynki, niezależnie od tego, jak wygląda teren pod nimi. W kodzie: build level. |
| Prędkość gry | Ile razy szybciej niż normalnie działają reguły. Zmienia liczbę ticków na sekundę czasu rzeczywistego, a nie to, co dzieje się w jednym ticku. |
| Promień | Wyobrażona linia prosta od kamery przez wskaźnik myszy. Pozwala ustalić, na co wskazuje gracz. |
| Przyciąganie do siatki | Przesunięcie wskazanego miejsca do najbliższego dozwolonego, tutaj do całych komórek siatki. W kodzie: snapping. |
| Pull request | Prośba o włączenie gałęzi do `main`. Zmiany są przedtem sprawdzane. |
| ScriptableObject | Zasób Unity przechowujący dane (np. koszt budynku), które można zmieniać w Inspectorze. |
| Symulacja | Część kodu, która przechowuje reguły i stan gry, niezależnie od tego, co widać na ekranie. |
| Test Edit Mode | Test automatyczny uruchamiany w edytorze Unity bez wciskania Play. |
| Tick | Jeden stały krok symulacji (0,1 s czasu gry). |
| Zdarzenie | Komunikat wysyłany przez jedną część gry („aktywowano kopułę”), na który mogą zareagować inne części. |
