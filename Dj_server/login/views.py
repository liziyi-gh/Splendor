from django.http import HttpResponse
from django.http.response import HttpResponseBadRequest
from django.views.decorators.csrf import csrf_exempt
from django.shortcuts import render


@csrf_exempt
def index(request):
    dict = {
        "loginFeedback": True,
    }
    if request.method == 'POST':
        return HttpResponse()
        return HttpResponse("yes")

    return HttpResponse()


# Create your views here.
