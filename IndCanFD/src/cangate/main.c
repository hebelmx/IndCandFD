#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/select.h>
#include <termios.h>
#include <linux/can.h>
#include <linux/can/raw.h>
#include "can_operations.h"
#include "database_operations.h"
#include "gpio_operations.h"

int main(void) {
    // ... Open the CAN sockets ...

  
    int id = 1;
    char* port = read_port_from_db(id);
    if (port) {
        printf("Using port: %s\n", port);
        // TODO: Add your main loop here
       
    } else {
        fprintf(stderr, "Could not retrieve port from database.\n");
        return 1;
    }

    // Set the terminal to raw mode to allow reading single characters
    struct termios old_tio, new_tio;
    tcgetattr(STDIN_FILENO, &old_tio);
    new_tio = old_tio;
    new_tio.c_lflag &= ~(ICANON | ECHO);
    tcsetattr(STDIN_FILENO, TCSANOW, &new_tio);

    while (1) {
        fd_set readset;
        FD_ZERO(&readset);  // Clear the set
        FD_SET(STDIN_FILENO, &readset);  // Add the standard input to the set
        //FD_SET(0, &readset);   // Add the can0 socket to the set
        //FD_SET(1, &readset);  // Add the can1 socket to the set
        // TODO: Add the GPIO file descriptor to the set

         // Wait for activity on any of the file descriptors
        if (select(STDIN_FILENO + 1, &readset, NULL, NULL, NULL) < 0) {
            perror("Error in select");
            break;
        }

// Check if there was activity on the standard input
        if (FD_ISSET(STDIN_FILENO, &readset)) {
            char c;

            //if (read(STDIN_FILENO, &c, 1) > 0) {

            ssize_t n = read(STDIN_FILENO, &c, 1);

            if (n < 0) {
                perror("Error in read");
                break;
            } else if (n == 0) {
                continue;
            }

                 // Skip newline characters
                if (c == '\n') {
                    continue;
                }

                if (  c == 'q') {
                    fprintf(stderr, "Exiting...\n");
                    break;
                }

                if (c >= '0' && c <= '9') {  // Check if 'c' is a digit
                int id = c - '0';  // Convert the digit character to an integer
                char* data = read_data_from_db(id);
                if (data) {
                    printf("Data associated with ID %d: %s\n", id, data);

                    struct can_frame frame;
                    fill_can_frame(&frame, data);

                    print_can_frame(&frame); 
                    send_frame(&frame, port);


                    free(data);
                } else {
                    printf("No data found or an error occurred for ID %d.\n", id);
                }
            } 

        }
    }

    // Restore the old terminal settings
    tcsetattr(STDIN_FILENO, TCSANOW, &old_tio);

    // ... Close the sockets ...
    free(port);  // Remember to free the memory allocated for the port string
    return 0;
    
}

