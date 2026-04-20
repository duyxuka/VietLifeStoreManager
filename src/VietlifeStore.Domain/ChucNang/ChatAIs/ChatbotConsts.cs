using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietlifeStore.ChucNang.ChatAIs
{
    public static class ChatbotConsts
    {
        public const string DefaultModel = "gemini-2.5-flash";   // ← Đổi sang Gemini
        public const int MaxContextMessages = 20;               // Giữ nguyên hoặc tăng lên 30-40 nếu muốn
        public const int MaxTokens = 8192;                      // Gemini hỗ trợ output lớn hơn nhiều
        public const string DefaultSystemPrompt =
            "You are a helpful, concise, and friendly AI assistant.";
    }
}
