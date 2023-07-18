#ifndef CAN_OPERATIONS_H
#define CAN_OPERATIONS_H

#include <linux/can.h>

// Function to send a CAN frame
int send_can_frame(int s, struct can_frame* frame);

// Function to read a CAN frame and compare it with another frame
int read_and_compare_can_frame(int s, struct can_frame* frame);

#endif // CAN_OPERATIONS_H
