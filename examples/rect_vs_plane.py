from pydoodle import *
import math

class RectVsPlane:
    def __init__(self):
        self.rect_centre=V2(0,0)
        self.rect_size=V2(0,0)
        self.rect_theta=0

        self.plane_theta=0
        self.plane_pos=V2(0,0)

        self._add_handles()

    def _add_handles(self):
        add_translate_handles(attrs(self,"rect_centre"))
        add_translate_handles(attrs(self,"rect_size"))
        add_rotate_handles(attrs(self,"rect_theta"))

        add_rotate_handles(attrs(self,"plane_theta"))
        add_translate_handles(attrs(self,"plane_pos"))

    def tick(self):
        plane_n=V2.from_angle(self.plane_theta)
        plane_d=-V2.dot(self.plane_pos,plane_n)

        rect_half=self.rect_size.x*V2.from_angle(self.rect_theta)+self.rect_size.y*V2.from_angle(self.rect_theta).perp()
        rect_half*=0.5
        rect_half=V2(abs(rect_half.x),
                     abs(rect_half.y))

        rect_dot_n=V2.dot(self.rect_centre,plane_n)

        rect_half_dot_n=V2.dot(rect_half,plane_n)

        rect_on_plane=self.rect_centre-(rect_dot_n+plane_d)*plane_n

        #
        print "rect_centre=%s rect_half=%s"%(self.rect_centre,rect_half)
        print "plane_n=%s plane_d=%s"%(plane_n,plane_d)
        print
        print "rect_dot_n=%s"%rect_dot_n
        print "rect_half_dot_n=%s"%rect_half_dot_n
        print "rect_on_plane=%s"%rect_on_plane
        print "diff len=%s"%((self.rect_centre-rect_on_plane).len())

        # draw plane
        set_colour(Colour(1,0,0))
        line(self.plane_pos-plane_n.perp()*100,
             self.plane_pos+plane_n.perp()*100)

        set_colour(Colour(.5,0,0))
        line(self.plane_pos,
             self.plane_pos+plane_n*100)

        # draw rect
        set_colour(Colour(0,0,0))
        line(self._get_rect_pt(0),self._get_rect_pt(1))
        line(self._get_rect_pt(1),self._get_rect_pt(3))
        line(self._get_rect_pt(3),self._get_rect_pt(2))
        line(self._get_rect_pt(2),self._get_rect_pt(0))

        set_colour(Colour(.5,.5,.5))
        line(self.rect_centre,self.rect_centre+rect_half)

        set_colour(Colour(.5,0,.5))
        line(self.rect_centre,rect_on_plane)
        
        self._tick_handles()

    def _get_rect_pt(self,i):
        x=-self.rect_size.x*.5

        if i&1:
            x=-x

        y=-self.rect_size.y*.5

        if i&2:
            y=-y

        xa=V2.from_angle(self.rect_theta)
        ya=xa.perp()

        return self.rect_centre+x*xa+y*ya

    def _tick_handles(self):
        self.rect_centre_handle.tick()
        self.rect_size_handle.tick()
        self.rect_theta_handle.tick(self.rect_centre)

        self.plane_pos_handle.tick()
        self.plane_theta_handle.tick(self.plane_pos)

run(RectVsPlane())
