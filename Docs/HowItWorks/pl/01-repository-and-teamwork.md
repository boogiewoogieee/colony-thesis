# 01 · Repozytorium i praca zespołowa

**W jednym zdaniu:** projekt jest przechowywany w repozytorium git na GitHubie, gdzie każda zmiana powstaje na osobnej gałęzi i trafia do `main` przez sprawdzony pull request.

## Po co nam to

Cztery osoby zmieniają ten sam projekt Unity. Bez kontroli wersji nadpisywalibyśmy sobie nawzajem pracę, tracilibyśmy historię i nie moglibyśmy cofnąć zepsutej zmiany. GitHub daje nam też wspólną tablicę zadań i zapis tego, kto co zrobił.

## Jak to działa

1. **Repozytorium** przechowuje wszystkie pliki potrzebne do otwarcia projektu: `Assets/`, `Packages/`, `ProjectSettings/` oraz nasze dokumenty. Pliki, które Unity odtwarza na każdym komputerze (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, pliki projektu IDE), są wypisane w `.gitignore` i nigdy nie trafiają do repozytorium.
2. **Gałęzie.** `main` zawsze zawiera najnowszą działającą wersję. Do każdego zadania tworzysz gałąź z `main` (np. `art/silo-model`), zapisujesz na niej swoją pracę w commitach i wysyłasz ją na GitHuba.
3. **Pull requesty.** Na GitHubie otwierasz pull request ze swojej gałęzi do `main`. Tymoteusz go sprawdza i łączy opcją **Squash and merge**, która zamienia całą gałąź w jeden commit na `main`, zatytułowany tak jak pull request.
4. **Chroniona gałąź `main`.** Zestaw reguł (ruleset) na GitHubie blokuje wysyłanie zmian bezpośrednio do `main`, wymuszone nadpisywanie historii (force push) i usunięcie tej gałęzi, a do tego wymaga liniowej historii. Nawet właściciel repozytorium korzysta z pull requestów, więc nic nie trafia do `main` bez sprawdzenia.
5. **Git LFS.** Git powstał z myślą o plikach tekstowych. Pliki binarne (`.png`, `.fbx`, `.wav`, `.psd`, czcionki) są duże i przy każdej edycji zmieniają się w całości, przez co historia szybko by puchła. Plik `.gitattributes` kieruje je do Git LFS: w repozytorium zostaje mały wskaźnik, a właściwy plik jest przechowywany osobno i pobierany wtedy, gdy jest potrzebny.
6. **Blokowanie plików.** Binarnych plików źródłowych nie da się połączyć, więc gdyby dwie osoby edytowały ten sam plik `.psd`, praca jednej z nich by przepadła. Takie typy plików są oznaczone jako `lockable`: polecenie `git lfs lock <plik>` rezerwuje plik do czasu odblokowania.
7. **Pliki meta.** Unity tworzy dla każdego zasobu plik `.meta` z jego unikalnym identyfikatorem (GUID). Sceny i prefaby odwołują się do zasobów właśnie przez ten identyfikator, dlatego plik `.meta` trzeba zawsze commitować razem z jego zasobem. Utrata tego pliku psuje wszystkie odwołania do zasobu.
8. **Zapis w formie tekstu.** Unity jest ustawione tak, żeby zapisywać sceny, prefaby i zasoby jako tekst (YAML). Dzięki temu git może pokazywać i łączyć zmiany linia po linii. Dla tych plików w `.gitattributes` zarejestrowane jest `UnityYAMLMerge`, narzędzie Unity do łączenia zmian, bo rozumie ich strukturę lepiej niż zwykłe łączenie tekstu.
9. **Opisy commitów** są zgodne z konwencją Conventional Commits (`feat(economy): add global storage`), dzięki czemu historię czyta się jak listę zmian i można ją filtrować według typu i modułu.

## Dlaczego takie rozwiązanie

| Możliwość | Dlaczego nie (albo dlaczego tak) |
| --- | --- |
| Dysk współdzielony albo pliki zip | Brak historii, brak łączenia zmian, ciągłe nadpisywanie. |
| Unity Version Control (Plastic) | Dobrze radzi sobie z plikami binarnymi, ale zespół zna już GitHuba, a jego darmowy plan, pull requesty i tablica projektu pokrywają nasze potrzeby. |
| Git bez LFS | Repozytorium rosłoby przy każdej zmianie tekstury, a jego pobieranie (klonowanie) stałoby się wolne. |
| Wysyłanie zmian prosto do `main` | Szybkie, ale jeden błąd psuje projekt wszystkim. Pull requesty dodają etap sprawdzenia prawie bez kosztu. |
| Zwykłe łączenie gałęzi (merge commit) zamiast squash | Na `main` zostawałby każdy drobny, roboczy commit. Squash utrzymuje `main` czytelnym: jeden commit na jedno ukończone zadanie. |

## Gdzie to jest w projekcie

- `.gitignore`: pliki, których git nigdy nie przechowuje.
- `.gitattributes`: znaki końca linii, typy plików w LFS, typy z blokowaniem, narzędzie do łączenia zmian.
- `README.md`: kroki konfiguracji i zasady zespołu.
- Ustawienia GitHuba: zestaw reguł „Protect main”, łączenie wyłącznie przez squash, etykiety, tablica projektu.

## Skąd wiemy, że działa

- `git lfs ls-files` wypisuje pliki przechowywane w LFS.
- Próba wysłania zmian bezpośrednio do `main` jest odrzucana przez GitHuba.
- Sklonowanie repozytorium na nowym komputerze i otwarcie go w dokładnie tej samej wersji Unity odtwarza projekt.

## Pytania, które mogą paść

**Dlaczego niektóre pliki widać na GitHubie jako „wskaźniki”?** Są przechowywane w Git LFS. GitHub pokazuje właściwy plik po jego otwarciu, a samo repozytorium przechowuje tylko odwołanie do niego.

**Co się stanie, gdy dwie osoby edytują tę samą scenę?** Git próbuje połączyć zmiany. Dzięki UnityYAMLMerge proste zmiany łączą się automatycznie. Złożonych konfliktów unikamy na dwa sposoby: umawiamy się, że scenę edytuje w danej chwili tylko jedna osoba, a obiekty wielokrotnego użytku umieszczamy w prefabach.

**Dlaczego folderu `Library/` nie ma w repozytorium?** To pamięć podręczna, którą Unity odbudowuje z zasobów na każdym komputerze. Przechowywanie jej dodałoby gigabajty danych i powodowało ciągłe konflikty.

**Dlaczego wszyscy muszą używać dokładnie tej samej wersji Unity?** Różne wersje zapisują wiele plików w nieco innych formatach. Zaśmieca to każdy commit przypadkowymi zmianami i może zepsuć zasoby.

**Skąd wiadomo, kto co zmienił?** Każdy commit zapisuje autora i datę, a każda zmiana w `main` jest powiązana z pull requestem, który ma opis.
