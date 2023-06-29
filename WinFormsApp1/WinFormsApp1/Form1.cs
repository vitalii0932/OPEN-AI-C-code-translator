using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using static WinFormsApp1.Form1;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public string apiKey1 = "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"; //��������� ���� ��� ����������

        static public string endpoint = "https://api.openai.com/v1/chat/completions"; //����� ��� ����������
        static public List<Message> messages = new List<Message>(); //������ ����������� ����������
        static public List<string> parts = new List<string>();

        int i = 0;

        public string fileText; //����� �������� � �����
        public string resultText;

        public List<string> resultTXT = new List<string>(); //��������� ����� �� ChatGPT

        //������ ��� ���������� �� ���������� �����
        public OpenFileDialog openFileDialog1 = new OpenFileDialog();
        public OpenFileDialog saveFileDialog1 = new OpenFileDialog();

        public class text
        {
            List<string> textParts = new List<string>(); //������ ���� ������� ������������
            List<string> textParts2 = new List<string>(); //��������� ��� ��������
            int j = 0;
            bool end;

            public string fullText = ""; //����� � ����� ��� �������� ������������
            public string msg = ""; //�������� �� �����������

            public List<string> GetPartsOfText()
            {
                textParts.Clear();
                textParts2.Clear();
                try
                {
                    textParts = fullText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    for (int i = 0; i < textParts.Count; i++)
                    {
                        textParts2.Add("");
                        for (; j < textParts.Count; j++)
                        {
                            if ((textParts2[i] + textParts[j] + msg).Length < 4000)
                            {
                                textParts2[i] = textParts2[i] + " /n " + textParts[j];
                                if (j == textParts.Count - 1) end = true;
                            }
                            else
                                break;
                        }
                        if (end) break;
                    }

                    for (int i = 0; i < textParts2.Count; i++)
                        textParts2[i] = msg + " " + textParts2[i];

                    return textParts2;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return textParts2;
                }
            }
        }

        //����� ��� ������� ����������� �����������
        public class Message
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = "";
            [JsonPropertyName("content")]
            public string Content { get; set; } = "";
        }
        class Request
        {
            [JsonPropertyName("model")]
            public string ModelId { get; set; } = "";
            [JsonPropertyName("messages")]
            public List<Message> Messages { get; set; } = new();
        }

        class ResponseData
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = "";
            [JsonPropertyName("object")]
            public string Object { get; set; } = "";
            [JsonPropertyName("created")]
            public ulong Created { get; set; }
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();
            [JsonPropertyName("usage")]
            public Usage Usage { get; set; } = new();
        }

        class Choice
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }
            [JsonPropertyName("message")]
            public Message Message { get; set; } = new();
            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; } = "";
        }

        class Usage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }
            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }
            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            //������������  textBox1
            textBox1.AutoSize = false;
            textBox1.Size = new System.Drawing.Size(618, 49);

            richTextBox2.MaxLength = 2147483647;

            //������������ �������
            openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            this.Text = "start";
            if (i == 0)
            {
                resultTXT.Clear();
                resultText = "";
            }
            text text = new text();

            button1.Enabled = false; //��������� ������
            button1.Text = "��������� �������";

            if (richTextBox1.Text.Length > 0) text.fullText = richTextBox1.Text; //�������� ����� ������ �����
            else text.fullText = fileText;

            if (textBox1.Text.Length == 0) //�������� ��������� ��������
            {
                MessageBox.Show("������ �����������");
                button1.Enabled = true;
                button1.Text = "������";
                return;
            }
            string msg = textBox1.Text; //��������

            text.msg = msg; //������� ����������� �����

            parts = text.GetPartsOfText(); //��������� ������ ����������

            this.Text = "10%";

            for (; i < parts.Count;)
            {
                messages.Clear();
                var content = parts[i]; //����� �����������

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey1}"); //����������

                this.Text = "20%";

                if (content is not { Length: > 0 }) //�������� �� ����� �����������
                {
                    button1.Enabled = true;
                    button1.Text = "������";
                    return;
                }

                this.Text = "40%";

                var message = new Message() { Role = "user", Content = content }; //��������� ����������� ����������� ��� ��������
                messages.Add(message);

                var requestData = new Request() //���������
                {
                    ModelId = "gpt-3.5-turbo-0301",
                    Messages = messages
                };

                this.Text = "60%";

                using var response = await httpClient.PostAsJsonAsync(endpoint, requestData); //�������� �����������

                if (!response.IsSuccessStatusCode) //�������� �� ��������� ����������
                {
                    MessageBox.Show($"{(int)response.StatusCode} {response.StatusCode}");
                    button1.Enabled = true;
                    button1.Text = "������";
                    return;
                }

                this.Text = "80%";

                ResponseData? responseData = await response.Content.ReadFromJsonAsync<ResponseData>(); //��������� �����������

                var choices = responseData?.Choices ?? new List<Choice>();
                if (choices.Count == 0) //�������� �� ����������� ������
                {
                    button1.Enabled = true;
                    button1.Text = "������";
                    MessageBox.Show("No choices were returned by the API");
                }
                var choice = choices[0];
                var responseMessage = choice.Message;
                messages.Add(responseMessage);
                string responseText = responseMessage.Content.Trim(); //�������� �����������

                resultTXT.Add(responseText); //������� ����������� �� ������

                List<string> textParts = new List<string>();

                textParts = resultTXT[i].Split(new string[] { "/n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int j = 0; j < textParts.Count; j++)
                {
                    textParts[j] = "" + textParts[j] + "\n";
                    resultText = resultText + textParts[j]; //����� ����������� �� �����
                }

                richTextBox2.Text = "";
                richTextBox2.Text = resultText;

                i++;
                this.Text = "100%";
                httpClient.Dispose();
            }
            MessageBox.Show("�������� ���������");
            button1.Enabled = true;
            button1.Text = "������";
            this.Text = "�������� ���������";
            i = 0;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) //�������� ����� �� ���� ����� �� �����
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                string filename = openFileDialog1.FileName;
                fileText = System.IO.File.ReadAllText(filename);
                richTextBox1.Text = fileText;

                MessageBox.Show("���� ������");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) //���������� �����
        {
            try
            {
                List<string> textParts = new List<string>();

                textParts = resultTXT[i].Split(new string[] { "/n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                textParts = resultTXT[i].Split(new string[] { "<n>" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                string text = "";
                foreach (var item in textParts) text = text + "\t" + item + "\n";

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = "|.txt";
                saveFileDialog.Filter = "test|*.txt";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog.FileName.Length > 0)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, true))
                    {
                        sw.WriteLine("\t" + text);
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_ClientSizeChanged(object sender, EventArgs e) //���� ������ ����
        {
            richTextBox1.Size = new System.Drawing.Size((this.ClientSize.Width - 55) / 2, (this.ClientSize.Height - 190));
            richTextBox2.Size = new System.Drawing.Size((this.ClientSize.Width - 55) / 2, (this.ClientSize.Height - 190));
            richTextBox2.Left = richTextBox1.Width + 55 - 20;
            label2.Left = richTextBox1.Width + 55 - 20;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}