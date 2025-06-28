#include <stdio.h>

int main()
{
	printf("Hello World!\n");
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file

// Native implementation of the RC4 algorithm

#define STATE_ARRAY_SIZE 256

// Shadow state array
char shadowS[STATE_ARRAY_SIZE] = { 0 };

// State array
char S[STATE_ARRAY_SIZE] = { 0 };

void InitShadowS() {
	for (size_t i = 0; i < STATE_ARRAY_SIZE; i++)
	{
		shadowS[i] = (char)i;
	}
}

// Key scheduling algorithm (KSA)
void Init(char key[], size_t key_length) {
	memcpy(S, shadowS, STATE_ARRAY_SIZE);

	char temp;
	int j = 0;

	for (size_t i = 0; i < STATE_ARRAY_SIZE; i++) {
		j = (j + S[i] + key[i % key_length]) & 0xFF;

		temp = S[i];
		S[i] = S[j];
		S[j] = temp;
	}
}

// Encryption
void Encrypt(char input[], size_t input_length, char output[]) {
	char temp;
	int i = 0;
	int j = 0;

	for (size_t cnt = 0; cnt < input_length; cnt++)
	{
		i = (i + 1) & 0xFF;
		j = (j + S[i]) & 0xFF;

		// Swap S[i] and S[j]
		temp = S[i];
		S[i] = S[j];
		S[j] = temp;

		output[cnt] = (char)(input[cnt] ^ S[(S[i] + S[j]) & 0xFF]);
	}
}
