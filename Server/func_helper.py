import threading

def static_vars(**kwargs):

    def decorate(func):
        for k in kwargs:
            setattr(func, k, kwargs[k])
        return func

    return decorate


def thread_safe(function):

    @static_vars(lock=threading.Lock())
    def async_wrapper(self, *args, **kwargs):
        with async_wrapper.lock:
            ret = function(self, *args, **kwargs)
            return ret

    return async_wrapper
