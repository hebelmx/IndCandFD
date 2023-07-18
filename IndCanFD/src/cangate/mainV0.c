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

int mainV0(void) {
    // ... Open the CAN sockets ...

    struct can_frame frame;

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

                if (  c == '1') {                
                    int s = 1;
                    // Prepare a CAN frame to send
                    struct can_frame frame;
                    frame.can_id = 0x001;  // CAN identifier
                    frame.can_dlc = 8;     // Data length code (number of bytes)
                    frame.data[0] = 0x01;  // Data byte 1
                    frame.data[1] = 0x02;  // Data byte 2
                    // ...
                    // Set other data bytes

                    print_can_frame(&frame); 
                    int size = sizeof(frame);

                    //send can frame 
                    //end_can_frame(&frame,size); // send the CAN frame
                    send_frame();
                }

                if ( c == '2') {
                
                    int id = 1;  // Specify the ID you want to read data for
                    char* data = read_data_from_db(id);
                    if (data) {
                        printf("Data associated with ID %d: %s\n", id, data);
                        free(data);  // Since strdup was used in read_data_from_db, we need to free the result to prevent memory leak
                    } else {
                        printf("No data found or an error occurred.\n");
                    }
                }   
                if (  c == 'q') {
                    fprintf(stderr, "Exiting...\n");
                    break;
                }

                if (c == '3') {
                    int id = 1;
                    char* data = read_data_from_db(id);
                    if (data) {
                        printf("Data associated with ID %d: %s\n", id, data);
                        
                        struct can_frame frame;
                        fill_can_frame(&frame, data);

                        print_can_frame(&frame); 
                        send_frame();

                        free(data);
                    } else {
                        printf("No data found or an error occurred.\n");
                    }
}

        }
    }

    // Restore the old terminal settings
    tcsetattr(STDIN_FILENO, TCSANOW, &old_tio);

    // ... Close the sockets ...

    return 0;
    
}

