int main(int argc, char *argv[])
{
	int i = 0;

	cliInit();

	if (argc == 1)
	{
		char* dummy_argv[2];
		dummy_argv[0] = argv[0];
		dummy_argv[1] = "inputtest";
		doInputTest(2, dummy_argv);   // call doInputTest with 1 and "inputtest" 
		return 0;                     // end the program after doInputTest has run
	}
	for (i = 0; i < CMD_ARRAY_SIZE; i++)
	{
		if ( (gCmdArray[i].name != NULL) && (gCmdArray[i].namePos < argc))
		{
			if (strcasecmp(argv[gCmdArray[i].namePos], gCmdArray[i].name) == 0)
			{
				gCmdArray[i].pFunc(argc, argv);
				return 0;
			}
		}
	}
	printf("Invalid command option\n");
	printf("%s\n", usage);

	return 0;
}




