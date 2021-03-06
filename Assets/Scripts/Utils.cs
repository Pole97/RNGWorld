using System.Collections;
using System.Collections.Generic;
using Unity.Burst;

public class Utils {
    public static float[,] ShiftMat(int n, int axis, float[,] m) {
        if (n > 0) {
            //Take the last row/column
            float[] saved = new float[m.GetLength(0)];
            for (int j = 0; j < m.GetLength(0); j++) {
                if (axis == 0) {
                    saved[j] = m[m.GetLength(0) - 1, j];
                } else {
                    saved[j] = m[j, m.GetLength(0) - 1];
                }
            }

            //Shift the other rows/columns
            for (int i = m.GetLength(0) - 1; i >= n; i--) {
                for (int j = 0; j < m.GetLength(0); j++) {
                    if (axis == 0) {
                        m[i, j] = m[i - n, j];
                    } else {
                        m[j, i] = m[j, i - n];
                    }
                }
            }

            //Replace the row/column saved to the first one
            for (int j = 0; j < m.GetLength(0); j++) {
                if (axis == 0) {
                    m[0, j] = saved[j];
                } else {
                    m[j, 0] = saved[j];
                }
            }
        } else if (n < 0) {
            //Take the last row/column
            float[] salvata = new float[m.GetLength(0)];
            for (int j = 0; j < m.GetLength(0); j++) {
                if (axis == 0) {
                    salvata[j] = m[0, j];
                } else {
                    salvata[j] = m[j, 0];
                }
            }

            //Shift the other rows/columns
            for (int i = 0; i < m.GetLength(0) + n; i++) {
                for (int j = 0; j < m.GetLength(0); j++) {
                    if (axis == 0) {
                        m[i, j] = m[i - n, j];
                    } else {
                        m[j, i] = m[j, i - n];
                    }
                }
            }

            //Replace the row/column saved to the last one
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

    public static bool LinearBound2(float a, float b, float minA, float minB) {
        return a / minA + b / minB >= 1f;
    }

    public static bool LinearBound3(float a, float b, float c, float minA, float minB, float minC) {
        return a / minA + b / minB + c / minC >= 1f;
    }

    public static float[,] ReverseMatrix(float[,] m) {
        for (int i = 0; i < m.GetLength(0); i++) {
            for (int j = 0; j < m.GetLength(1); j++) {
                m[i, j] = -m[i, j];
            }
        }
        return m;
    }
}
