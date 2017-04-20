using System;
using System.Collections.Generic;

namespace BoinShell {
    public static class TabComplete {

        public static List<string> history = new List<string>();
        static int _historySize = 100;

        public static int historySize {
            get { return _historySize; }
            set {
                _historySize = value;

                if (history.Count > historySize) {
                    while (history.Count != historySize) {
                        history.RemoveAt(history.Count - 1);
                    }
                }
            }
        }

        public static string readLine(List<string> list, int lineStart = 0, ConsoleColor color = ConsoleColor.DarkGray) {
            var startingColor = Console.ForegroundColor;

            string line = "";
            string suggestion = "";

            // last command entered
            int historyPos = -1;

            while (true) {
                bool usedHistory = false;
                var input = Console.ReadKey();

                if (input.Key == ConsoleKey.Enter) {
                    break;

                } else if (input.Key == ConsoleKey.Backspace) {
                    if (line != "") {
                        line = line.Substring(0, line.Length - 1);
                    }

                } else if (input.Key == ConsoleKey.Tab) {
                    if (suggestion != "") {
                        line = suggestion;
                    }
                
                } else if (input.Key == ConsoleKey.UpArrow) {
                    string historyItem = getHistoryItem(1, ref historyPos);
                    if (historyItem != "") {
                        line = historyItem;
                    }

                } else if (input.Key == ConsoleKey.DownArrow) {
                    if (historyPos <= 0) {
                        line = "";
                        historyPos = -1;
                    } else {
                        string historyItem = getHistoryItem(-1, ref historyPos);
                        if (historyItem != "") {
                            line = historyItem;
                        }
                    }

                } else {
                    line += input.KeyChar;
                }

                suggestion = "";
                if (!usedHistory) {

                    string lineLower = line.ToLower();
                    foreach (string item in list) {
                        if (item.ToLower().StartsWith(lineLower)) {
                            suggestion = item;
                            break;
                        }
                    }
                }

                clearLine(lineStart);
                Console.Write(line);

                if (line != "" && suggestion != "") {
                    Console.ForegroundColor = color;

                    // only the part of the suggestion they haven't written
                    Console.Write(suggestion.Substring(line.Length, suggestion.Length - line.Length));

                    Console.ForegroundColor = startingColor; 
                }
            }

            clearLine(lineStart);
            Console.WriteLine(line);

            if (line != "") {
                addCommand(line);
            }

            return line;
        }

        static void clearLine(int lineStart) {
            int currentLine = Console.CursorTop;

            Console.SetCursorPosition(lineStart, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth - lineStart));

            // oh boy, here's a hack
            Console.SetCursorPosition(
                lineStart, 

                // if we're at the BufferHeight - 1, we need to actually be at the BufferHeight - 2???
                // otherwise we get some crazy behavior
                (currentLine == Console.BufferHeight - 1) ? Console.BufferHeight - 2 : currentLine
            );
        }

        static string getHistoryItem(int up, ref int pos) {
            if (history.Count == 0) {
                pos = -1;
                return "";
            }

            pos += up;

            if (pos < 0) {
                pos = 0;
            } else if (pos > history.Count - 1) {
                pos = history.Count - 1;
            }

            return history[pos];
        }

        static void addCommand(string command) {
            history.Insert(0, command);

            // a bit hacky, but it *will* dequeue all that go over the size
            historySize = historySize;
        }
    }
}
