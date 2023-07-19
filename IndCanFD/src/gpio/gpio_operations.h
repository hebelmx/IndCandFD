#ifndef GPIO_OPERATIONS_H
#define GPIO_OPERATIONS_H

// This function retrieves the unique ID associated with the current state of the GPIO.
int get_id_from_gpio();

// This function checks for changes in the GPIO's ID state.
// It takes an integer argument which corresponds to the previous state of the ID.
int get_id_changed_from_gpio(int prev_id_state);

// This function retrieves the current trigger state from the GPIO.
int get_trigger_from_gpio();

// This function checks for changes in the GPIO's trigger state.
// It takes an integer argument which corresponds to the previous state of the trigger.
int get_trigger_changed_from_gpio(int prev_trigger_state);

// This function sets a signal to indicate that all operations are proceeding as expected.
int set_signal_ok();

// This function sets a signal to indicate that a message has been successfully sent.
int set_signal_message_sended();

// This function sets a signal to indicate that an error occurred while sending a message.
int set_signal_message_error();

// This function sets a signal to indicate that an error has occurred within the module.
int set_signal_module_error();

#endif // GPIO_OPERATIONS_H

