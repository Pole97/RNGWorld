using System.Collections;
using System.Collections.Generic;
using Unity.Burst;

public class Utils {
    public static float[,] ShiftMat(int n, int axis, float[,] m) {
        if (n > 0) {
            //Prendo l'ultima riga/colonna
            float[] salvata = new float[m.GetLength(0)];
            for (int j = 0; j < m.GetLength(0); j++) {
                if (axis == 0) {
                    salvata[j] = m[m.GetLength(0) - 1, j];
                } else {
                    salvata[j] = m[j, m.GetLength(0) - 1];
                }
            }

            //sposto le altre righe/colonne
            for (int i = m.GetLength(0) - 1; i >= n; i--) {
                for (int j = 0; j < m.GetLength(0); j++) {
                    if (axis == 0) {
                        m[i, j] = m[i - n, j];
                    } else {
                        m[j, i] = m[j, i - n];
                    }
                }
            }

            //rimetto la riga/colonna salvata
            for (int j = 0; j < m.GetLength(0); j++) {
                if (axis == 0) {
                    m[0, j] = salvata[j];
                } else {
                    m[j, 0] = salvata[j];
                }
            }
        } else if (n < 0) {
            //Prendo la prima riga/colonna
            float[] salvata = new float[m.GetLength(0)];
            for (int j = 0; j < m.GetLength(0); j++) {
                if (axis == 0) {
                    salvata[j] = m[0, j];
                } else {
                    salvata[j] = m[j, 0];
                }
            }

            //sposto le altre righe/colonne
            for (int i = 0; i < m.GetLength(0) + n; i++) {
                for (int j = 0; j < m.GetLength(0); j++) {
                    if (axis == 0) {
                        m[i, j] = m[i - n, j];
                    } else {
                        m[j, i] = m[j, i - n];
                    }
                }
            }

            //sostituisco la riga/colonna salvata alla prima riga
            for (int j = 0; j < m.GetLength(0); j++) {
                if (axis == 0) {
                    m[m.GetLength(0) - 1, j] = salvata[j];
                } else {
                    m[j, m.GetLength(0) - 1] = salvata[j];
                }
            }
        }
        return m;
    }

    public static bool linearBound2(float a, float b, float minA, float minB) {
        return a / minA + b / minB >= 1f;
    }

    public static bool linearBound3(float a, float b, float c, float minA, float minB, float minC) {
        return a / minA + b / minB + c / minC >= 1f;
    }
}
