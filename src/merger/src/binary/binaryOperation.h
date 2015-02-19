#ifndef BINARYOPERATION_H
#define BINARYOPERATION_H

#include "../utils/stream.h"

namespace binary {
    class BinaryOperation {
    protected:
        std::shared_ptr<utils::Stream> _stream;
        int _pointer_size;
        int _array_count_size;

    public:
        BinaryOperation(std::shared_ptr<utils::Stream> stream, int pointer_size, int array_count_size) {
            this->_stream = stream;
            this->_pointer_size = pointer_size;
            this->_array_count_size = array_count_size;
        }

        utils::Stream* baseStream() {
            return this->_stream.get();
        }
    };
}

#endif
