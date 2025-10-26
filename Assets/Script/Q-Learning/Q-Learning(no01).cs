using System;

class QLearningTest
{
    static Random random = new Random();
    static int stateCount = 3; // Số trạng thái
    static int actionCount = 3; // Số hành động
    static double[,] qTable = new double[stateCount, actionCount]; // Bảng Q
    static double alpha = 0.1; // Learning rate
    static double gamma = 0.9; // Discount factor
    static double epsilon = 0.1; // Epsilon-greedy

    // Danh sách trạng thái và hành động
    static string[] states = { "LowArmy", "MediumArmy", "HighArmy" };
    static string[] actions = { "WeakMonster", "MediumMonster", "StrongMonster" };

    static void Main(string[] args)
    {
        int episodes = 1000; // Số lần lặp học
        for (int i = 0; i < episodes; i++)
        {
            // Chọn trạng thái ngẫu nhiên
            int state = random.Next(stateCount);

            // Chọn hành động
            int action = ChooseAction(state);

            // Nhận phần thưởng từ môi trường
            double reward = CalculateReward(state, action);

            // Trong ví dụ này, trạng thái tiếp theo không thay đổi
            int nextState = state;

            // Cập nhật giá trị Q
            UpdateQTable(state, action, reward, nextState);
        }

        // In bảng Q
        PrintQTable();

        // Hiển thị hành động tối ưu từ mỗi trạng thái
        for (int s = 0; s < stateCount; s++)
        {
            int bestAction = GetBestAction(s);
            Console.WriteLine($"Từ trạng thái '{states[s]}', hành động tối ưu là '{actions[bestAction]}'");
        }
    }

    // Hàm chọn hành động theo epsilon-greedy
    static int ChooseAction(int state)
    {
        if (random.NextDouble() < epsilon)
        {
            // Khám phá: chọn hành động ngẫu nhiên
            return random.Next(actionCount);
        }
        else
        {
            // Khai thác: chọn hành động có giá trị Q cao nhất
            return GetBestAction(state);
        }
    }

    // Hàm tính phần thưởng
    static double CalculateReward(int state, int action)
    {
        if (state == 0 && action == 0) return 10; // LowArmy -> WeakMonster
        if (state == 1 && action == 1) return 20; // MediumArmy -> MediumMonster
        if (state == 2 && action == 2) return 30; // HighArmy -> StrongMonster
        return -10; // Sai kết hợp
    }

    // Hàm cập nhật giá trị Q
    static void UpdateQTable(int state, int action, double reward, int nextState)
    {
        double currentQ = qTable[state, action];
        double maxNextQ = GetMaxQValue(nextState);
        qTable[state, action] = currentQ + alpha * (reward + gamma * maxNextQ - currentQ);
    }

    // Lấy giá trị Q cao nhất từ trạng thái
    static double GetMaxQValue(int state)
    {
        double maxQ = double.MinValue;
        for (int a = 0; a < actionCount; a++)
        {
            if (qTable[state, a] > maxQ)
            {
                maxQ = qTable[state, a];
            }
        }
        return maxQ;
    }

    // Lấy hành động tốt nhất từ trạng thái
    static int GetBestAction(int state)
    {
        int bestAction = 0;
        double maxQ = double.MinValue;
        for (int a = 0; a < actionCount; a++)
        {
            if (qTable[state, a] > maxQ)
            {
                maxQ = qTable[state, a];
                bestAction = a;
            }
        }
        return bestAction;
    }

    // In bảng Q
    static void PrintQTable()
    {
        Console.WriteLine("Q-Table:");
        for (int s = 0; s < stateCount; s++)
        {
            for (int a = 0; a < actionCount; a++)
            {
                Console.Write($"{qTable[s, a]:F2} ");
            }
            Console.WriteLine();
        }
    }
}
/*
 * Trạng thái (States):

LowArmy, MediumArmy, HighArmy đại diện cho mức chiến lực của người chơi.

Hành động (Actions):

WeakMonster, MediumMonster, StrongMonster đại diện cho loại quái vật được sinh ra.

Phần thưởng (Reward):

Xác định mức độ phù hợp giữa trạng thái và hành động (ví dụ: nếu chiến lực thấp mà sinh ra quái yếu thì thưởng cao).

Q-Table:

Lưu trữ giá trị Q cho mỗi cặp trạng thái-hành động.

Epsilon-Greedy:

Kết hợp giữa khám phá (random) và khai thác (tìm giá trị Q cao nhất).

Cập nhật giá trị Q:




 */